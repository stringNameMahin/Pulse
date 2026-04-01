const { app, BrowserWindow, ipcMain } = require("electron");
const path = require("path");
const { spawn } = require("child_process");

function createWindow() {
  const win = new BrowserWindow({
    show: false,
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  win.loadURL("http://localhost:5173");

  win.setMenu(null);

  win.once("ready-to-show", () => {
    win.maximize();
    win.show();
    win.webContents.openDevTools();
  });
}

ipcMain.on("restart-as-admin", () => {
  const exePath = app.getPath("exe");

  spawn("powershell", [
    "-Command",
    `Start-Process "${exePath}" -Verb runAs`
  ], {
    detached: true,
    stdio: "ignore"
  });

  app.quit();
});

app.whenReady().then(createWindow);