import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const Header: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="app-header">
      <div className="header-content">
        <div className="header-left">
          <Link to="/" className="header-logo">
            <h1>BasarMapApp</h1>
          </Link>
        </div>
        
        <div className="header-right">
          {user && (
            <>
              <div className="user-info">
                <span className="username">{user.username}</span>
                <span className={`user-role role-${user.role.toLowerCase()}`}>
                  {user.role}
                </span>
              </div>
              
              {user.role === 'Admin' && (
                <>
                  <Link to="/admin/users" className="admin-link">
                    Kullanıcı Yönetimi
                  </Link>
                  <Link to="/admin/logs" className="admin-link">
                    Sistem Logları
                  </Link>
                </>
              )}
              
              <button onClick={handleLogout} className="logout-button">
                Logout
              </button>
            </>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;
