import { API_BASE } from "../../../shared/api/apiBase";

export interface IndexStats {
  indexName: string;
  documentCount: number;
  sizeBytes?: number;
  isHealthy: boolean;
}

export async function getIndexStatus(): Promise<IndexStats> {
  const res = await fetch(`${API_BASE}/api/indexing/status`);
  if (!res.ok) { throw new Error(`HTTP ${res.status}`); }
  return res.json() as Promise<IndexStats>;
}
