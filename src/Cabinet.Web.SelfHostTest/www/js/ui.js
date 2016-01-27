
(function (window, Cabinet) {
    function getBrowseListContainer() {
        return document.getElementById('browse-files-list');
    }

    function renderFiles(files) {
        var html = [
            '<div>'
        ];

        for (var i = 0; i < files.length; ++i) {
            var file = files[i];
            html.push([
                '<div>',
                file.Key,
                '</div>'
            ].join(''));
        }

        html.push('</div>');

        var htmlStr = html.join('');

        getBrowseListContainer().innerHTML = htmlStr;
    }

    function renderError(error) {
        getBrowseListContainer().innerHTML = 'There was an error listing the files';
    }

    function listFiles() {
        return Cabinet.browse
                    .getFiles({ keyPrefix: '' })
                    .then(renderFiles)
                    .catch(renderError)
    }

    var browseRefreshButton = document.getElementById('browse-refresh');
    browseRefreshButton.addEventListener('click', listFiles);

    // Upload
    var form = document.getElementById('upload-form');

    form.addEventListener('submit', function (submitEvt) {
        var resultDisplay = document.getElementById('result-message');

        resultDisplay.innerHTML = 'Uploading...';

        Cabinet.upload.uploadFile(form).then(function (response) {
            listFiles();
            resultDisplay.innerHTML = "Uploaded!";
        }).catch(function (err) {
            resultDisplay.innerHTML = "Error " + req.status + " occurred when trying to upload your file.<br \/>";
        });        

        submitEvt.preventDefault();
    }, false);
})(window, window.Cabinet);