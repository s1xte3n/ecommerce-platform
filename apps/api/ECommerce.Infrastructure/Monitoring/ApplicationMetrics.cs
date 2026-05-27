// File: apps/api/src/Infrastructure/Monitoring/ApplicationMetrics.cs
using System.Diagnostics.Metrics;

namespace ECommerce.Infrastructure.Monitoring;

/// <summary>
/// Custom application metrics for observability.
/// </summary>
public static class ApplicationMetrics
{
    private static readonly Meter Meter = new("ECommerce.Api", "1.0.0");

    // Counters
    public static readonly Counter<int> OrdersCreated = Meter.CreateCounter<int>(
        "orders_created_total",
        "orders",
        "Total number of orders created");

    public static readonly Counter<int> PaymentsProcessed = Meter.CreateCounter<int>(
        "payments_processed_total",
        "payments",
        "Total number of payments processed");

    public static readonly Counter<int> ProductsCreated = Meter.CreateCounter<int>(
        "products_created_total",
        "products",
        "Total number of products created");

    // Histograms
    public static readonly Histogram<double> CheckoutDuration = Meter.CreateHistogram<double>(
        "checkout_duration_seconds",
        "seconds",
        "Time taken to complete checkout");

    public static readonly Histogram<double> ApiRequestDuration = Meter.CreateHistogram<double>(
        "api_request_duration_seconds",
        "seconds",
        "API request duration");

    // Gauges
    public static readonly ObservableGauge<int> ActiveUsers = Meter.CreateObservableGauge(
        "active_users",
        () => 0, // Replace with actual implementation
        "users",
        "Number of currently active users");

    public static readonly ObservableGauge<int> CartAbandonmentRate = Meter.CreateObservableGauge(
        "cart_abandonment_rate",
        () => 0, // Replace with actual implementation
        "percent",
        "Cart abandonment rate percentage");
}
