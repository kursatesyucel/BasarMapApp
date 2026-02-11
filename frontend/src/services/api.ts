import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5012/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Token'ı her istekte GÜNCEL olarak localStorage'dan çeker
// Bu sayede login olduktan sonra sayfayı refresh yapmaya gerek kalmaz
api.interceptors.request.use(
  (config) => {
    // Her istekte token'ı fresh olarak al (login sonrası manuel refresh gerektirmez)
    const token = localStorage.getItem('token');
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    } else {
      // Token yoksa Authorization header'ını kaldır (temizlik için)
      delete config.headers.Authorization;
    }
    
    return config;
  },
  (error) => {
    console.error('Request interceptor error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor - Hataları yakalar ve 401'de otomatik login'e yönlendirir
api.interceptors.response.use(
  (response) => {
    // Başarılı response'ları olduğu gibi döndür
    return response;
  },
  (error) => {
    // 401 Unauthorized - Token geçersiz veya süresi dolmuş
    if (error.response?.status === 401) {
      console.warn('401 Unauthorized - Token geçersiz veya süresi dolmuş. Login sayfasına yönlendiriliyor...');
      
      // Auth data'yı temizle
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      // Kullanıcıyı login sayfasına yönlendir
      const currentPath = window.location.pathname;
      
      // Eğer zaten auth sayfalarında değilse, mevcut sayfayı redirect parametresi olarak kaydet
      // Login sonrası kullanıcı kaldığı yere dönebilir
      if (
        currentPath !== '/login' && 
        currentPath !== '/register' && 
        currentPath !== '/verify-email'
      ) {
        window.location.href = `/login?redirect=${encodeURIComponent(currentPath)}`;
      } else {
        window.location.href = '/login';
      }
    }
    
    // 403 Forbidden - Yetkisiz erişim (kullanıcı login ama yetki yok)
    if (error.response?.status === 403) {
      console.warn('403 Forbidden - Bu işlem için yetkiniz yok');
      // 403'te logout yapma, sadece hata mesajı göster
    }
    
    // 500 Internal Server Error
    if (error.response?.status === 500) {
      console.error('500 Internal Server Error:', error.response.data);
    }
    
    // Network error (backend çalışmıyor)
    if (!error.response) {
      console.error('Network Error - Backend\'e ulaşılamıyor:', error.message);
    }
    
    return Promise.reject(error);
  }
); 