import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { authService } from '../services/authService';
import { UserListItem } from '../types';

const AdminPanel: React.FC = () => {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [updatingUserId, setUpdatingUserId] = useState<number | null>(null);
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
      await authService.updateUserRole(userId, { role: newRole });
      await loadUsers(); // Reload users after update
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to update user role';
      setError(errorMessage);
      console.error('Error updating user role:', err);
    } finally {
      setUpdatingUserId(null);
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
              <th>Role</th>
              <th>Created At</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.length === 0 ? (
              <tr>
                <td colSpan={5} className="no-users">
                  No users found
                </td>
              </tr>
            ) : (
              users.map((userItem) => (
                <tr key={userItem.id}>
                  <td>{userItem.id}</td>
                  <td>
                    <strong>{userItem.username}</strong>
                    {userItem.username === user?.username && (
                      <span className="current-user-badge">You</span>
                    )}
                  </td>
                  <td>
                    <span className={`role-badge role-${userItem.role.toLowerCase()}`}>
                      {userItem.role}
                    </span>
                  </td>
                  <td>{new Date(userItem.createdAt).toLocaleDateString()}</td>
                  <td>
                    <select
                      value={userItem.role}
                      onChange={(e) => handleRoleChange(userItem.id, e.target.value)}
                      disabled={updatingUserId === userItem.id}
                      className="role-select"
                    >
                      <option value="User">User</option>
                      <option value="Manager">Manager</option>
                      <option value="Admin">Admin</option>
                    </select>
                    {updatingUserId === userItem.id && (
                      <span className="updating-indicator">Updating...</span>
                    )}
                  </td>
                </tr>
              ))
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
          <div className="stat-value">{users.filter(u => u.role === 'Admin').length}</div>
          <div className="stat-label">Admins</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{users.filter(u => u.role === 'Manager').length}</div>
          <div className="stat-label">Managers</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{users.filter(u => u.role === 'User').length}</div>
          <div className="stat-label">Users</div>
        </div>
      </div>
      </div>
    </div>
  );
};

export default AdminPanel;
