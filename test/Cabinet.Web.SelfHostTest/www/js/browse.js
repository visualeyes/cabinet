(function (window, Cabinet) {
    function getFiles(query) {
        query = query || {};
        var params = Object.keys(query)
                   .map((key) => encodeURIComponent(key) + "=" + encodeURIComponent(query[key]))
                   .join("&")
                   .replace(/%20/g, "+");

        var url = '/api/browse?' + params; 

        return fetch(url, { method: 'get' })
            .then(function (response) {
                return response.json();
            });
    }


    Cabinet.browse = {
        getFiles: getFiles
    };
})(window, window.Cabinet);