

using Prometheus;

namespace NotificationService.Metrics;

public static class NotificationMetrics
{
    public static readonly Counter NotificationsSent = Prometheus.Metrics.CreateCounter("notifications_sent_total", "Liczba wysłanych powiadomień", new CounterConfiguration
        {
            LabelNames = new[] { "channel" }
        });

    public static readonly Counter NotificationsFailed = Prometheus.Metrics.CreateCounter("notifications_failed_total", "Nieudane próby wysyłki", new CounterConfiguration
        {
            LabelNames = new[] { "channel" }
        });

    public static readonly Gauge PendingNotifications = Prometheus.Metrics
        .CreateGauge("notifications_pending_total", "Liczba oczekujących powiadomień", new GaugeConfiguration
        {
            LabelNames = new[] { "channel" }
        });
}
