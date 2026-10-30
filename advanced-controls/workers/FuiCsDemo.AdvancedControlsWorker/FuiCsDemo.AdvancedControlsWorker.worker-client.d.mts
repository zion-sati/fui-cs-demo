export interface NetWasmWorkerCallOptions {
  signal?: AbortSignal;
}
export interface NetWasmWorkerClient {
  "__fui_worker_cancel"(options?: NetWasmWorkerCallOptions): Promise<void>;
  "__fui_worker_run"(entryName: string, input: string, options?: NetWasmWorkerCallOptions): Promise<string>;
  dispose(): Promise<void>;
  terminate(): void;
}
export interface NetWasmWorkerStartupOptions { environment?: Record<string, string>; stdout?: (line: string) => void; stderr?: (line: string) => void; onNotification?: (value: { operation: string; arguments: unknown[] }) => void | Promise<void>; }
export declare function createWorker(options?: NetWasmWorkerStartupOptions): Promise<NetWasmWorkerClient>;
