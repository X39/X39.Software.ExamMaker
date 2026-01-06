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
