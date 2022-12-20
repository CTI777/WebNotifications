var graphapi = '/api/graph/';

var mGraph = null;
var priceHolder = null;

var data = [],
    totalPoints = 300;

function getApiUrl(action) {
    return graphapi + (action ? action : '');
}

function addNewDataValue(nd) {
    if (!nd) return;

    var si = data.length - totalPoints - 1;
    if (si > 0) data = data.slice(si);

    si = data.length > 0 ? data[data.length - 1][0] : -1;

    data.push([si + 1, nd]);
}

function updatePrice(d) {
    if (priceHolder) {
        var el = document.getElementById(priceHolder);
        if (el) {
            if (!(el.value==null)) el.value = ('$' + d);
            if (!(el.innerHTML==null)) el.innerHTML = ('$' + d);
        }
    }
}

function update(d) {
    updatePrice(d);

    addNewDataValue(d);
    if (mGraph) {
        mGraph.update(data);
    }
}

function init(e, graphHolderId, priceHolderId) {
    if (priceHolderId) priceHolder = priceHolderId;
    if (graphHolderId) mGraph = new graph(graphHolderId);

    if (window.EventSource == undefined) {
        // If not supported  
        console.log("Your browser doesn't support Server Sent Events.");
        return;
    } else {

        if (!!window.EventSource) {

            var source = new EventSource(getApiUrl());

            source.addEventListener('message', function (e) {
                console.log(e.data);

                update(parseInt(e.data, 10));

            }, false);

            source.addEventListener('open', function (e) {
                console.log("open!");

            }, false);

            source.addEventListener('error', function (e) {
                if (e.readyState == EventSource.CLOSED) {
                    console.log('error: Connection Closed.');

                }
            }, false);

        } else {
            console.log("Not supported!");

        }
    }
}


window.addEventListener('load', function () { init(event, 'graphHolder', 'priceHolder'); });
