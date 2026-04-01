import { createContext, useContext, useState } from "react";

const ToastContext = createContext();

export const useToast = () => useContext(ToastContext);

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const addToast = (message, type = "info") => {
    const id = Date.now();

    setToasts(prev => [...prev, { id, message, type }]);

    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 3000);
  };

  return (
    <ToastContext.Provider value={{ addToast }}>
      {children}

      {/* TOAST UI */}
      <div style={{
        position: "fixed",
        bottom: 20,
        right: 20,
        display: "flex",
        flexDirection: "column",
        gap: 10,
        zIndex: 9999
      }}>
        {toasts.map(t => (
          <div
            key={t.id}
            style={{
              padding: "10px 14px",
              borderRadius: 10,
              background:
                t.type === "error" ? "#ef4444" :
                t.type === "success" ? "#22c55e" :
                "#334155",
              color: "#fff",
              animation: "fadeSlide 0.3s ease"
            }}
          >
            {t.message}
          </div>
        ))}
      </div>

      <style>{`
        @keyframes fadeSlide {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
        }
      `}</style>

    </ToastContext.Provider>
  );
}