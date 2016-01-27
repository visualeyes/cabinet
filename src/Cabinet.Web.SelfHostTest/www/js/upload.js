
(function (window, Cabinet) {
    function uploadFile(form) {
        return fetch('/api/upload', {
            method: 'post',
            body: new FormData(form)
        })
    }

    Cabinet.upload = {
        uploadFile: uploadFile
    }
})(window, window.Cabinet);