export function ApiDownload(url, fileName, jwt) {

    let element = document.createElement('a');
    element.style.display = 'none';
    document.body.appendChild(element);
    element.setAttribute('target', '_blank');
    element.setAttribute('download', fileName)
    let headers = new Headers();
    headers.append('Authorization', "Bearer " + jwt);

    fetch(url, {headers})
        .then(response => response.blob())
        .then(blobby => {
            try {

                let objectUrl = window.URL.createObjectURL(blobby);
                
                element.href = objectUrl;
                element.click();

                window.URL.revokeObjectURL(objectUrl);
            } finally {
                element.remove();
            }
        });
}

export function Confirm(message) {
    return confirm(message);
}

export function Download(fileName, mimeType, content) {
    const blob = new Blob([content], {type: mimeType});
    const url = URL.createObjectURL(blob);
    const element = document.createElement('a');
    element.style.display = 'none';
    document.body.appendChild(element);
    element.setAttribute('href', url);
    element.setAttribute('download', fileName);
    element.click();
    window.URL.revokeObjectURL(url);
    element.remove();
}

export function Click(id) {
    document.getElementById(id).click();
}
