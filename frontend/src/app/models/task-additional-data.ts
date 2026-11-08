export interface TaskAdditionalData {
  priority?: string;
  estimatedEndDate?: string;
  tags?: string[];
  metadata?: Record<string, unknown>;
}
