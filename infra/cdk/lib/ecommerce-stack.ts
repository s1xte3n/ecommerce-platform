// File: infra/cdk/lib/ecommerce-stack.ts
import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as ecs_patterns from 'aws-cdk-lib/aws-ecs-patterns';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as elasticache from 'aws-cdk-lib/aws-elasticache';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import * as cloudwatch from 'aws-cdk-lib/aws-cloudwatch';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as iam from 'aws-cdk-lib/aws-iam';
import { Construct } from 'constructs';

interface ECommerceStackProps extends cdk.StackProps {
  environment: string;
}

export class ECommerceStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props: ECommerceStackProps) {
    super(scope, id, props);

    const isProduction = props.environment === 'production';

    // ============================================
    // VPC
    // ============================================
    const vpc = new ec2.Vpc(this, 'Vpc', {
      maxAzs: 3,
      natGateways: isProduction ? 3 : 1,
      subnetConfiguration: [
        {
          name: 'Public',
          subnetType: ec2.SubnetType.PUBLIC,
          cidrMask: 24,
        },
        {
          name: 'Private',
          subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
          cidrMask: 24,
        },
        {
          name: 'Isolated',
          subnetType: ec2.SubnetType.PRIVATE_ISOLATED,
          cidrMask: 24,
        },
      ],
    });

    // ============================================
    // Security Groups
    // ============================================
    const apiSecurityGroup = new ec2.SecurityGroup(this, 'ApiSecurityGroup', {
      vpc,
      description: 'Security group for ECS API tasks',
      allowAllOutbound: true,
    });

    const dbSecurityGroup = new ec2.SecurityGroup(this, 'DbSecurityGroup', {
      vpc,
      description: 'Security group for RDS PostgreSQL',
      allowAllOutbound: false,
    });

    dbSecurityGroup.addIngressRule(
      apiSecurityGroup,
      ec2.Port.tcp(5432),
      'Allow API to access PostgreSQL'
    );

    const redisSecurityGroup = new ec2.SecurityGroup(this, 'RedisSecurityGroup', {
      vpc,
      description: 'Security group for ElastiCache Redis',
      allowAllOutbound: false,
    });

    redisSecurityGroup.addIngressRule(
      apiSecurityGroup,
      ec2.Port.tcp(6379),
      'Allow API to access Redis'
    );

    // ============================================
    // Secrets Manager
    // ============================================
    const dbCredentials = new secretsmanager.Secret(this, 'DbCredentials', {
      secretName: `ecommerce/db-credentials/${props.environment}`,
      generateSecretString: {
        secretStringTemplate: JSON.stringify({
          username: 'ecommerce_admin',
        }),
        generateStringKey: 'password',
        excludeCharacters: '"@/\\\'',
        passwordLength: 32,
      },
    });

    // ============================================
    // RDS PostgreSQL
    // ============================================
    const dbInstance = new rds.DatabaseInstance(this, 'PostgresInstance', {
      engine: rds.DatabaseInstanceEngine.postgres({
        version: rds.PostgresEngineVersion.VER_16,
      }),
      instanceType: isProduction
        ? ec2.InstanceType.of(ec2.InstanceClass.R6G, ec2.InstanceSize.LARGE)
        : ec2.InstanceType.of(ec2.InstanceClass.T4G, ec2.InstanceSize.MICRO),
      vpc,
      vpcSubnets: {
        subnetType: ec2.SubnetType.PRIVATE_ISOLATED,
      },
      securityGroups: [dbSecurityGroup],
      credentials: rds.Credentials.fromSecret(dbCredentials),
      multiAz: isProduction,
      allocatedStorage: isProduction ? 100 : 20,
      maxAllocatedStorage: isProduction ? 500 : 100,
      backupRetention: isProduction
        ? cdk.Duration.days(30)
        : cdk.Duration.days(7),
      deletionProtection: isProduction,
      removalPolicy: isProduction
        ? cdk.RemovalPolicy.RETAIN
        : cdk.RemovalPolicy.DESTROY,
      enablePerformanceInsights: true,
      performanceInsightRetention: isProduction
        ? rds.PerformanceInsightRetention.MONTHS_2
        : rds.PerformanceInsightRetention.DEFAULT,
      cloudwatchLogsExports: ['postgresql'],
      cloudwatchLogsRetention: logs.RetentionDays.THREE_MONTHS,
    });

    // ============================================
    // ElastiCache Redis
    // ============================================
    const redisSubnetGroup = new elasticache.CfnSubnetGroup(this, 'RedisSubnetGroup', {
      description: 'Subnet group for Redis cluster',
      subnetIds: vpc.privateSubnets.map(subnet => subnet.subnetId),
      cacheSubnetGroupName: `ecommerce-redis-subnet-${props.environment}`,
    });

    const redisCluster = new elasticache.CfnCacheCluster(this, 'RedisCluster', {
      engine: 'redis',
      cacheNodeType: isProduction
        ? 'cache.r6g.large'
        : 'cache.t4g.micro',
      numCacheNodes: 1,
      clusterName: `ecommerce-redis-${props.environment}`,
      vpcSecurityGroupIds: [redisSecurityGroup.securityGroupId],
      cacheSubnetGroupName: redisSubnetGroup.cacheSubnetGroupName,
      snapshotRetentionLimit: isProduction ? 7 : 0,
    });

    // ============================================
    // ECS Cluster
    // ============================================
    const cluster = new ecs.Cluster(this, 'EcsCluster', {
      vpc,
      containerInsights: true,
      clusterName: `ecommerce-cluster-${props.environment}`,
    });

    // ============================================
    // ECS Task Role
    // ============================================
    const taskRole = new iam.Role(this, 'TaskRole', {
      assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
      description: `Role for ECS tasks in ${props.environment}`,
    });

    taskRole.addToPolicy(new iam.PolicyStatement({
      effect: iam.Effect.ALLOW,
      actions: ['secretsmanager:GetSecretValue'],
      resources: [dbCredentials.secretArn],
    }));

    taskRole.addToPolicy(new iam.PolicyStatement({
      effect: iam.Effect.ALLOW,
      actions: [
        'logs:CreateLogStream',
        'logs:PutLogEvents',
      ],
      resources: ['*'],
    }));

    // ============================================
    // ECS Task Definition
    // ============================================
    const taskDefinition = new ecs.FargateTaskDefinition(this, 'ApiTaskDef', {
      memoryLimitMiB: isProduction ? 2048 : 512,
      cpu: isProduction ? 1024 : 256,
      taskRole,
      executionRole: taskRole,
    });

    const container = taskDefinition.addContainer('ApiContainer', {
      image: ecs.ContainerImage.fromRegistry(
        `ghcr.io/your-org/ecommerce-platform/api:latest`
      ),
      logging: ecs.LogDrivers.awsLogs({
        streamPrefix: 'api',
        logRetention: logs.RetentionDays.THREE_MONTHS,
      }),
      environment: {
        ASPNETCORE_ENVIRONMENT: props.environment,
        Redis__ConnectionString: redisCluster.attrRedisEndpointAddress,
      },
      secrets: {
        ConnectionStrings__DefaultConnection: ecs.Secret.fromSecretsManager(
          dbCredentials,
          'connectionString'
        ),
      },
      healthCheck: {
        command: [
          'CMD-SHELL',
          'curl -f http://localhost:8080/health || exit 1',
        ],
        interval: cdk.Duration.seconds(30),
        timeout: cdk.Duration.seconds(5),
        retries: 3,
        startPeriod: cdk.Duration.seconds(60),
      },
    });

    container.addPortMappings({
      containerPort: 8080,
      protocol: ecs.Protocol.TCP,
    });

    // ============================================
    // Application Load Balancer + Fargate Service
    // ============================================
    const fargateService = new ecs_patterns.ApplicationLoadBalancedFargateService(
      this,
      'ApiService',
      {
        cluster,
        taskDefinition,
        desiredCount: isProduction ? 3 : 1,
        publicLoadBalancer: true,
        assignPublicIp: false,
        securityGroups: [apiSecurityGroup],
        listenerPort: 443,
        healthCheckGracePeriod: cdk.Duration.seconds(120),
      }
    );

    // Auto-scaling
    const scaling = fargateService.service.autoScaleTaskCount({
      minCapacity: isProduction ? 3 : 1,
      maxCapacity: isProduction ? 10 : 2,
    });

    scaling.scaleOnCpuUtilization('CpuScaling', {
      targetUtilizationPercent: 70,
      scaleInCooldown: cdk.Duration.seconds(300),
      scaleOutCooldown: cdk.Duration.seconds(60),
    });

    scaling.scaleOnMemoryUtilization('MemoryScaling', {
      targetUtilizationPercent: 80,
      scaleInCooldown: cdk.Duration.seconds(300),
      scaleOutCooldown: cdk.Duration.seconds(60),
    });

    // ============================================
    // CloudWatch Alarms
    // ============================================
    new cloudwatch.Alarm(this, 'HighCpuAlarm', {
      metric: fargateService.service.metricCpuUtilization(),
      threshold: 90,
      evaluationPeriods: 3,
      datapointsToAlarm: 2,
      comparisonOperator: cloudwatch.ComparisonOperator.GREATER_THAN_THRESHOLD,
      alarmDescription: 'Alert when API CPU exceeds 90%',
      treatMissingData: cloudwatch.TreatMissingData.NOT_BREACHING,
    });

    new cloudwatch.Alarm(this, 'HighMemoryAlarm', {
      metric: fargateService.service.metricMemoryUtilization(),
      threshold: 85,
      evaluationPeriods: 3,
      datapointsToAlarm: 2,
      comparisonOperator: cloudwatch.ComparisonOperator.GREATER_THAN_THRESHOLD,
      alarmDescription: 'Alert when API memory exceeds 85%',
      treatMissingData: cloudwatch.TreatMissingData.NOT_BREACHING,
    });

    new cloudwatch.Alarm(this, 'DatabaseConnectionAlarm', {
      metric: dbInstance.metricDatabaseConnections(),
      threshold: 80,
      evaluationPeriods: 3,
      datapointsToAlarm: 2,
      comparisonOperator: cloudwatch.ComparisonOperator.GREATER_THAN_THRESHOLD,
      alarmDescription: 'Alert when DB connections exceed 80%',
      treatMissingData: cloudwatch.TreatMissingData.NOT_BREACHING,
    });

    // ============================================
    // CloudWatch Dashboard
    // ============================================
    const dashboard = new cloudwatch.Dashboard(this, 'ECommerceDashboard', {
      dashboardName: `ECommerce-${props.environment}`,
    });

    dashboard.addWidgets(
      new cloudwatch.GraphWidget({
        title: 'API - CPU Utilization',
        left: [fargateService.service.metricCpuUtilization()],
        width: 12,
        period: cdk.Duration.minutes(1),
      }),
      new cloudwatch.GraphWidget({
        title: 'API - Memory Utilization',
        left: [fargateService.service.metricMemoryUtilization()],
        width: 12,
        period: cdk.Duration.minutes(1),
      }),
      new cloudwatch.GraphWidget({
        title: 'RDS - Database Connections',
        left: [dbInstance.metricDatabaseConnections()],
        width: 12,
        period: cdk.Duration.minutes(1),
      }),
      new cloudwatch.GraphWidget({
        title: 'RDS - CPU Utilization',
        left: [dbInstance.metricCPUUtilization()],
        width: 12,
        period: cdk.Duration.minutes(1),
      })
    );

    // ============================================
    // Outputs
    // ============================================
    new cdk.CfnOutput(this, 'ApiEndpoint', {
      value: fargateService.loadBalancer.loadBalancerDnsName,
      description: 'API Load Balancer DNS',
    });

    new cdk.CfnOutput(this, 'DbEndpoint', {
      value: dbInstance.dbInstanceEndpointAddress,
      description: 'Database endpoint',
    });

    new cdk.CfnOutput(this, 'RedisEndpoint', {
      value: redisCluster.attrRedisEndpointAddress,
      description: 'Redis endpoint',
    });
  }
}
