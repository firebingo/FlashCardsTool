function getDimensions() {
    return {
        Width: parseInt(window.innerWidth),
        Height: parseInt(window.innerHeight)
    }
}

function getCssVar(varName) {
    return window.getComputedStyle(document.body).getPropertyValue(varName);
}

function getLocalStorageCache(key) {
    const l = localStorage.getItem(key);
    if (l == null || l === '')
        return null;
    const j = JSON.parse(l);
    const now = Date();
    if (j.expireTimeUnix !== 0) {
        var e = Date(j.expireTimeUnix);
        if (now > e)
            return null;
    }

    return j.value;
}

function setLocalStorageCache(key, item) {
    localStorage.setItem(key, item)
}

function clearLocalStorageCacheItem(key) {
    localStorage.removeItem(key);
}

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}