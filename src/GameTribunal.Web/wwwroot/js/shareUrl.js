// shareUrl.js
// JavaScript interop module for Web Share API (RF-007)

/**
 * Checks if the Web Share API is supported in the current browser.
 * @returns {boolean} True if Web Share API is supported, false otherwise.
 */
export function isShareSupported() {
    return navigator.share !== undefined;
}

/**
 * Shares a URL using the native Web Share API.
 * @param {string} url - The URL to share
 * @param {string} title - The title of the share dialog
 * @param {string} text - The text description to share
 * @returns {Promise<boolean>} Promise that resolves to true if successful, false otherwise
 */
export async function shareUrl(url, title, text) {
    if (!isShareSupported()) {
        console.warn('Web Share API is not supported in this browser');
        return false;
    }

    try {
        await navigator.share({
            title: title,
            text: text,
            url: url
        });
        return true;
    } catch (error) {
        // User cancelled the share or an error occurred
        if (error.name === 'AbortError') {
            console.log('Share was cancelled by the user');
        } else {
            console.error('Error sharing:', error);
        }
        return false;
    }
}

/**
 * Copies text to clipboard as a fallback when Web Share API is not supported.
 * @param {string} text - The text to copy to clipboard
 * @returns {Promise<boolean>} Promise that resolves to true if successful, false otherwise
 */
export async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (error) {
        console.error('Failed to copy to clipboard:', error);
        
        // Fallback for older browsers
        try {
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            document.body.appendChild(textArea);
            textArea.select();
            const success = document.execCommand('copy');
            document.body.removeChild(textArea);
            return success;
        } catch (fallbackError) {
            console.error('Fallback copy also failed:', fallbackError);
            return false;
        }
    }
}
