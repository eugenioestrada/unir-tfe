// sessionStorage.js
// JavaScript interop module for browser sessionStorage access (RF-017)

export function setItem(key, value) {
    try {
        sessionStorage.setItem(key, value);
        return true;
    } catch (e) {
        console.error('Failed to save to sessionStorage:', e);
        return false;
    }
}

export function getItem(key) {
    try {
        return sessionStorage.getItem(key);
    } catch (e) {
        console.error('Failed to read from sessionStorage:', e);
        return null;
    }
}

export function removeItem(key) {
    try {
        sessionStorage.removeItem(key);
        return true;
    } catch (e) {
        console.error('Failed to remove from sessionStorage:', e);
        return false;
    }
}

export function clear() {
    try {
        sessionStorage.clear();
        return true;
    } catch (e) {
        console.error('Failed to clear sessionStorage:', e);
        return false;
    }
}
