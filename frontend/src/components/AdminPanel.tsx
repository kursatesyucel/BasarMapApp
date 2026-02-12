import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { authService } from '../services/authService';
import { UserListItem } from '../types';

const AdminPanel: React.FC = () => {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [updatingUserId, setUpdatingUserId] = useState<number | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<number | null>(null);
  const { user } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await authService.getAllUsers();
      setUsers(data);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to load users';
      setError(errorMessage);
      console.error('Error loading users:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleRoleChange = async (userId: number, newRole: string) => {
    try {
      setUpdatingUserId(userId);
      setError(null);
      setSuccess(null);
      await authService.updateUserRole(userId, { role: newRole });
      setSuccess('Role updated successfully');
      await loadUsers(); // Reload users after update
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to update user role';
      setError(errorMessage);
      console.error('Error updating user role:', err);
    } finally {
      setUpdatingUserId(null);
    }
  };

  const handleStatusToggle = async (userId: number, currentStatus: boolean, username: string) => {
    const action = currentStatus ? 'deactivate' : 'activate';
    const actionTr = currentStatus ? 'pasif yapmak' : 'aktif yapmak';
    
    if (!confirm(`${username} kullanıcısını ${actionTr} istediğinizden emin misiniz?`)) {
      return;
    }

    try {
      setUpdatingUserId(userId);
      setError(null);
      setSuccess(null);
      await authService.updateUserStatus(userId, { isActive: !currentStatus });
      setSuccess(`User ${action}d successfully`);
      await loadUsers();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || `Failed to ${action} user`;
      setError(errorMessage);
      console.error(`Error ${action}ing user:`, err);
    } finally {
      setUpdatingUserId(null);
    }
  };

  const handleDeleteUser = async (userId: number, username: string) => {
    if (!confirm(`⚠️ UYARI: ${username} kullanıcısını KALICI OLARAK silmek istediğinizden emin misiniz?\n\nBu işlem geri alınamaz!`)) {
      return;
    }

    // Double confirmation for delete
    if (!confirm('Bu kullanıcı veritabanından tamamen silinecek. Devam etmek istediğinizden emin misiniz?')) {
      return;
    }

    try {
      setDeletingUserId(userId);
      setError(null);
      setSuccess(null);
      await authService.deleteUser(userId);
      setSuccess('User deleted successfully');
      await loadUsers();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to delete user';
      setError(errorMessage);
      console.error('Error deleting user:', err);
    } finally {
      setDeletingUserId(null);
    }
  };

  if (loading) {
    return (
      <div className="admin-panel">
        <div className="admin-header">
          <h1>User Management</h1>
        </div>
        <div className="loading-container">
          <div className="loading-spinner">Loading users...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="admin-panel-container">
      <div className="admin-panel">
        <div className="admin-header">
        <h1>User Management</h1>
        <div className="admin-actions">
          <button onClick={() => navigate('/')} className="back-to-map-button">
            ← Back to Map
          </button>
          <button onClick={loadUsers} disabled={loading} className="refresh-button">
            {loading ? 'Refreshing...' : 'Refresh'}
          </button>
        </div>
      </div>

      {error && (
        <div className="admin-error">
          {error}
        </div>
      )}

      {success && (
        <div className="admin-success">
          {success}
        </div>
      )}

      <div className="admin-info">
        <p>
          <strong>Logged in as:</strong> {user?.username} ({user?.role})
        </p>
        <p className="role-description">
          <strong>Role Descriptions:</strong>
        </p>
        <ul className="role-list">
          <li><strong>User:</strong> Can manage points only</li>
          <li><strong>Manager:</strong> Can manage points and lines</li>
          <li><strong>Admin:</strong> Full access to all resources + user management</li>
        </ul>
      </div>

      <div className="users-table-container">
        <table className="users-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Username</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Email Verified</th>
              <th>Created At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.length === 0 ? (
              <tr>
                <td colSpan={8} className="no-users">
                  No users found
                </td>
              </tr>
            ) : (
              users.map((userItem) => {
                const isCurrentUser = userItem.username === user?.username;
                const isUpdating = updatingUserId === userItem.id;
                const isDeleting = deletingUserId === userItem.id;
                
                return (
                  <tr key={userItem.id} className={!userItem.isActive ? 'inactive-user-row' : ''}>
                    <td>{userItem.id}</td>
                    <td>
                      <strong>{userItem.username}</strong>
                      {isCurrentUser && (
                        <span className="current-user-badge">You</span>
                      )}
                    </td>
                    <td className="user-email">{userItem.email}</td>
                    <td>
                      <select
                        value={userItem.role}
                        onChange={(e) => handleRoleChange(userItem.id, e.target.value)}
                        disabled={isUpdating || isDeleting}
                        className="role-select"
                      >
                        <option value="User">User</option>
                        <option value="Manager">Manager</option>
                        <option value="Admin">Admin</option>
                      </select>
                    </td>
                    <td>
                      <div className="status-container">
                        <span className={`status-badge ${userItem.isActive ? 'status-active' : 'status-inactive'}`}>
                          {userItem.isActive ? '🟢 Active' : '🔴 Inactive'}
                        </span>
                        <button
                          onClick={() => handleStatusToggle(userItem.id, userItem.isActive, userItem.username)}
                          disabled={isCurrentUser || isUpdating || isDeleting}
                          className={`toggle-status-button ${userItem.isActive ? 'deactivate' : 'activate'}`}
                          title={isCurrentUser ? 'You cannot change your own status' : (userItem.isActive ? 'Deactivate user' : 'Activate user')}
                        >
                          {userItem.isActive ? '⏸ Deactivate' : '▶ Activate'}
                        </button>
                      </div>
                    </td>
                    <td>
                      <span className={`verification-badge ${userItem.isEmailConfirmed ? 'verified' : 'unverified'}`}>
                        {userItem.isEmailConfirmed ? '✓ Verified' : '✗ Not Verified'}
                      </span>
                    </td>
                    <td>{new Date(userItem.createdAt).toLocaleDateString()}</td>
                    <td>
                      <button
                        onClick={() => handleDeleteUser(userItem.id, userItem.username)}
                        disabled={isCurrentUser || isUpdating || isDeleting}
                        className="delete-user-button"
                        title={isCurrentUser ? 'You cannot delete your own account' : 'Delete user permanently'}
                      >
                        {isDeleting ? '⏳ Deleting...' : '🗑️ Delete'}
                      </button>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      <div className="admin-stats">
        <div className="stat-card">
          <div className="stat-value">{users.length}</div>
          <div className="stat-label">Total Users</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{users.filter(u => u.isActive).length}</div>
          <div className="stat-label">Active Users</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{users.filter(u => !u.isActive).length}</div>
          <div className="stat-label">Inactive Users</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{users.filter(u => u.role === 'Admin').length}</div>
          <div className="stat-label">Admins</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{users.filter(u => u.role === 'Manager').length}</div>
          <div className="stat-label">Managers</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{users.filter(u => u.role === 'User').length}</div>
          <div className="stat-label">Regular Users</div>
        </div>
      </div>
      </div>
    </div>
  );
};

export default AdminPanel;
