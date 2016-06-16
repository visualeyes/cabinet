
(function (window, Cabinet) {
    var keyPrefix = '';
    var browseList = document.getElementById('browse-files-list');

    var browseRefreshButton = document.getElementById('browse-refresh');
    browseRefreshButton.addEventListener('click', listFiles);

    // Upload
    var form = document.getElementById('upload-form');

    function getItemType(type) {
        switch (type) {
            case 0:
                return 'Directory';
            case 1:
                return 'File';
        }
    }

    function renderFiles(files) {
        var html = [];

        for (var i = 0; i < files.length; ++i) {
            var file = files[i];
            html.push([
                '<tr>',
                    '<td class="file-key">', file.Key, '</td>',
                    '<td>', getItemType(file.Type), '</td>',
                '</tr>'
            ].join(''));
        }

        var htmlStr = html.join('');

        browseList.innerHTML = htmlStr;
    }

    function renderError(error) {
        browseList.innerHTML = 'There was an error listing the files';
    }

    function listFiles() {
        return Cabinet.browse
                    .getFiles({ keyPrefix: keyPrefix })
                    .then(renderFiles)
                    .catch(renderError)
    }
    
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

    browseList.addEventListener('click', function (evt) {
        if (evt.target.className === 'file-key') {
            console.log(evt);
            keyPrefix = evt.target.innerText;
            listFiles();
        }
    });

    listFiles();

    Cabinet.ui = {
        listFiles: listFiles
    };
})(window, window.Cabinet);