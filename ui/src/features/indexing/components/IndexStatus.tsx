import { useState, useEffect } from "react";
import { getIndexStatus, type IndexStats } from "../api/indexingApi";

export function IndexStatus() {
  const [stats, setStats] = useState<IndexStats | null>(null);

  useEffect(() => {
    const load = () => getIndexStatus().then(setStats).catch(() => null);
    load();
    const interval = setInterval(load, 30_000);
    return () => clearInterval(interval);
  }, []);

  if (!stats) {
    return <div className="index-status loading">Connecting…</div>;
  }

  const sizeLabel = stats.sizeBytes
    ? stats.sizeBytes > 1_000_000
      ? `${(stats.sizeBytes / 1_000_000).toFixed(1)} MB`
      : `${(stats.sizeBytes / 1_000).toFixed(0)} KB`
    : null;

  return (
    <div className={`index-status ${stats.isHealthy ? "healthy" : "unhealthy"}`}>
      <span className="status-dot" />
      <span>
        {stats.documentCount.toLocaleString()} docs indexed
        {sizeLabel && ` · ${sizeLabel}`}
      </span>
    </div>
  );
}
