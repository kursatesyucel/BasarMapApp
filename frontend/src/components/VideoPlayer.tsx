import React, { useRef, useState, useEffect } from 'react';

interface VideoPlayerProps {
  videoUrl: string;
  poster?: string;
  width?: number | string;
  height?: number | string;
  autoPlay?: boolean;
  controls?: boolean;
  muted?: boolean;
  loop?: boolean;
  onLoadStart?: () => void;
  onCanPlay?: () => void;
  onError?: (error: string) => void;
  onProgress?: (buffered: number) => void;
}

const VideoPlayer: React.FC<VideoPlayerProps> = ({
  videoUrl,
  poster,
  width = '100%',
  height = 'auto',
  autoPlay = false,
  controls = true,
  muted = false,
  loop = false,
  onLoadStart,
  onCanPlay,
  onError,
  onProgress
}) => {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [bufferedPercentage, setBufferedPercentage] = useState(0);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    const handleLoadStart = () => {
      setIsLoading(true);
      setError(null);
      onLoadStart?.();
    };

    const handleCanPlay = () => {
      setIsLoading(false);
      onCanPlay?.();
    };

    const handleCanPlayThrough = () => {
      setIsLoading(false);
    };

    const handleError = () => {
      const errorMessage = 'Video yüklenirken hata oluştu';
      setError(errorMessage);
      setIsLoading(false);
      onError?.(errorMessage);
    };

    const handleProgress = () => {
      if (video.buffered.length > 0) {
        const bufferedEnd = video.buffered.end(video.buffered.length - 1);
        const duration = video.duration;
        if (duration > 0) {
          const buffered = (bufferedEnd / duration) * 100;
          setBufferedPercentage(buffered);
          onProgress?.(buffered);
        }
      }
    };

    const handleWaiting = () => {
      setIsLoading(true);
    };

    const handlePlaying = () => {
      setIsLoading(false);
    };

    // Event listener'ları ekle
    video.addEventListener('loadstart', handleLoadStart);
    video.addEventListener('canplay', handleCanPlay);
    video.addEventListener('canplaythrough', handleCanPlayThrough);
    video.addEventListener('error', handleError);
    video.addEventListener('progress', handleProgress);
    video.addEventListener('waiting', handleWaiting);
    video.addEventListener('playing', handlePlaying);

    return () => {
      // Cleanup
      video.removeEventListener('loadstart', handleLoadStart);
      video.removeEventListener('canplay', handleCanPlay);
      video.removeEventListener('canplaythrough', handleCanPlayThrough);
      video.removeEventListener('error', handleError);
      video.removeEventListener('progress', handleProgress);
      video.removeEventListener('waiting', handleWaiting);
      video.removeEventListener('playing', handlePlaying);
    };
  }, [onLoadStart, onCanPlay, onError, onProgress]);

  const containerStyle: React.CSSProperties = {
    position: 'relative',
    width: width,
    height: height,
    backgroundColor: '#000',
    borderRadius: '8px',
    overflow: 'hidden'
  };

  const videoStyle: React.CSSProperties = {
    width: '100%',
    height: '100%',
    objectFit: 'contain'
  };

  const overlayStyle: React.CSSProperties = {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: 'rgba(0, 0, 0, 0.7)',
    color: 'white',
    fontSize: '14px',
    zIndex: 10
  };

  const progressBarStyle: React.CSSProperties = {
    position: 'absolute',
    bottom: 0,
    left: 0,
    height: '3px',
    backgroundColor: '#2196F3',
    width: `${bufferedPercentage}%`,
    transition: 'width 0.3s ease',
    zIndex: 5
  };

  const progressBarBgStyle: React.CSSProperties = {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    height: '3px',
    backgroundColor: 'rgba(255, 255, 255, 0.3)',
    zIndex: 4
  };

  return (
    <div style={containerStyle}>
      <video
        ref={videoRef}
        src={videoUrl}
        poster={poster}
        autoPlay={autoPlay}
        controls={controls}
        muted={muted}
        loop={loop}
        style={videoStyle}
        preload="metadata" // Sadece meta veriler yüklensin, video chunk'lar halinde gelsin
      />
      
      {/* Buffer progress bar */}
      {bufferedPercentage > 0 && bufferedPercentage < 100 && (
        <>
          <div style={progressBarBgStyle} />
          <div style={progressBarStyle} />
        </>
      )}
      
      {/* Loading overlay */}
      {isLoading && !error && (
        <div style={overlayStyle}>
          <div style={{ textAlign: 'center' }}>
            <div style={{ 
              width: '24px', 
              height: '24px', 
              border: '2px solid #fff', 
              borderTop: '2px solid transparent', 
              borderRadius: '50%', 
              animation: 'spin 1s linear infinite',
              margin: '0 auto 8px'
            }} />
            Video yükleniyor...
          </div>
        </div>
      )}
      
      {/* Error overlay */}
      {error && (
        <div style={overlayStyle}>
          <div style={{ textAlign: 'center', color: '#f44336' }}>
            <div style={{ fontSize: '18px', marginBottom: '8px' }}>⚠️</div>
            {error}
          </div>
        </div>
      )}
      
      <style>
        {`
          @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
          }
        `}
      </style>
    </div>
  );
};

export default VideoPlayer; 