export interface UserDevice {
  id: string;
  deviceId: string;
  deviceName: string;
  ipAddress: string;
  lastLoginDate: string;
  firstSeenDate: string;
  isTrusted: boolean;
  isCurrentDevice: boolean;
}

export interface DevicesResponse {
  success: boolean;
  message: string;
  data: UserDevice[];
}
