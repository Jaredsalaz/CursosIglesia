// Helper functions for file downloads

window.downloadFile = function (data, filename) {
    const blob = new Blob([new Uint8Array(data)], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Share functionality
window.shareCertificate = function (text) {
    if (navigator.share) {
        navigator.share({
            title: 'Mi Certificado',
            text: text
        }).catch(err => console.log('Error sharing:', err));
    } else {
        // Fallback: copy to clipboard
        navigator.clipboard.writeText(text).then(() => {
            console.log('Copied to clipboard');
        }).catch(err => console.error('Failed to copy:', err));
    }
};
