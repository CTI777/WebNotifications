function graph(divId) {

    this.placeholder = divId;//id of the div

    this.g = 0; //instance of dyngraph



    this.init = function (data) {

        var el = document.getElementById(this.placeholder);

        if (!el) return;

        this.g = new Dygraph(el, data);

    };



    this.update = function (d) {

        if (this.g) {

            this.g.updateOptions({ 'file': d });

        } else {

            this.init(d);

        }

    }

}