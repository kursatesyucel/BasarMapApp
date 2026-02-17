import { api } from './api';
import type { LogFilter, LoginLog, AuditLog, PaginatedResult } from '../types/logTypes';
import type { ApiResponse } from '../types';

/**
 * Builds query string from filter object, excluding undefined/null/empty values
 */
function buildQueryParams(filter: LogFilter): URLSearchParams {
  const params = new URLSearchParams();

  if (filter.page != null && filter.page > 0) {
    params.append('page', String(filter.page));
  }
  if (filter.pageSize != null && filter.pageSize > 0) {
    params.append('pageSize', String(filter.pageSize));
  }
  if (filter.startDate) {
    params.append('startDate', filter.startDate);
  }
  if (filter.endDate) {
    params.append('endDate', filter.endDate);
  }
  if (filter.userId) {
    params.append('userId', filter.userId);
  }
  if (filter.isSuccess !== undefined && filter.isSuccess !== null) {
    params.append('isSuccess', String(filter.isSuccess));
  }
  if (filter.searchText?.trim()) {
    params.append('searchText', filter.searchText.trim());
  }
  if (filter.action?.trim()) {
    params.append('action', filter.action.trim());
  }
  if (filter.entityName?.trim()) {
    params.append('entityName', filter.entityName.trim());
  }

  return params;
}

export const logService = {
  async getLoginLogs(filter: LogFilter = {}): Promise<PaginatedResult<LoginLog>> {
    const params = buildQueryParams(filter);
    const queryString = params.toString();
    const url = queryString ? `/logs/login?${queryString}` : '/logs/login';
    const response = await api.get<ApiResponse<PaginatedResult<LoginLog>>>(url);
    return response.data.data!;
  },

  async getAuditLogs(filter: LogFilter = {}): Promise<PaginatedResult<AuditLog>> {
    const params = buildQueryParams(filter);
    const queryString = params.toString();
    const url = queryString ? `/logs/audit?${queryString}` : '/logs/audit';
    const response = await api.get<ApiResponse<PaginatedResult<AuditLog>>>(url);
    return response.data.data!;
  },

  async getUserLogs(userId: number, filter: LogFilter = {}): Promise<{ loginLogs: PaginatedResult<LoginLog>; auditLogs: PaginatedResult<AuditLog> }> {
    const params = buildQueryParams(filter);
    const queryString = params.toString();
    const url = queryString ? `/logs/user/${userId}?${queryString}` : `/logs/user/${userId}`;
    const response = await api.get<ApiResponse<{ loginLogs: PaginatedResult<LoginLog>; auditLogs: PaginatedResult<AuditLog> }>>(url);
    return response.data.data!;
  },
};
