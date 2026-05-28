export type AppConfig = {
  apiUrl: string;
};

declare global {
  interface Window {
    __APP_CONFIG__?: Partial<AppConfig>;
  }
}

export function getAppConfig(): AppConfig {
  return {
    apiUrl: window.__APP_CONFIG__?.apiUrl?.replace(/\/$/, '') ?? '',
  };
}
