/**
 * Log types - Backend LogFilterDto and API responses
 */

export interface LogFilter {
  page?: number;
  pageSize?: number;
  startDate?: string;
  endDate?: string;
  userId?: string;
  isSuccess?: boolean;
  searchText?: string;
  action?: string;
  entityName?: string;
}

export interface LoginLog {
  id?: string;
  userId?: number;
  email: string;
  timestamp: string;
  ipAddress?: string;
  userAgent?: string;
  isSuccess: boolean;
  failureReason?: string;
}

export interface AuditLog {
  id?: string;
  userId?: number;
  action: string;
  entityName: string;
  entityId?: string;
  timestamp: string;
  ipAddress?: string;
  newValues?: unknown;
}

export interface PaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
