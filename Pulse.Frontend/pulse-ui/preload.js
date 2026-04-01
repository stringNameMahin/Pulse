const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("electronAPI", {
  restartAsAdmin: () => ipcRenderer.send("restart-as-admin")
});