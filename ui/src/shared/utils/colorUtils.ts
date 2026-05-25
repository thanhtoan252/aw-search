const COLOR_MAP: Record<string, string> = {
  Black: "#1a1a1a",
  White: "#f5f5f5",
  Red: "#dc2626",
  Blue: "#2563eb",
  Yellow: "#eab308",
  Silver: "#94a3b8",
  Grey: "#6b7280",
};

export function colorToHex(name: string): string {
  return COLOR_MAP[name] ?? "#94a3b8";
}
