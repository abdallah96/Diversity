Diversity.controller('HomeCtrl', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {

    $scope.percentage = [];
    $scope.number = [];

    $scope.init = function () {
        $scope.GetPercentages();
        $scope.GetNumbers();
        $scope.GetEmployees();
    };

    function generateBarChart(data, elementId) {
        //sort bars based on value
        data = data.sort(function (a, b) {
            return d3.ascending(a.value, b.value);
        })

        //set up svg using margin conventions - we'll need plenty of room on the left for labels
        var margin = {
            top: 15,
            right: 25,
            bottom: 15,
            left: 120
        };

        var width = 350 - margin.left - margin.right,
            height = 200 - margin.top - margin.bottom;

        var svg = d3.select("#" + elementId).append("svg")
            .attr("width", width + margin.left + margin.right + 10)
            .attr("height", height + margin.top + margin.bottom)
            .append("g")
            .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

        var x = d3.scale.linear()
            .range([0, width])
            .domain([0, d3.max(data, function (d) {
                return d.value;
            })]);

        var y = d3.scale.ordinal()
            .rangeRoundBands([height, 0], .1)
            .domain(data.map(function (d) {
                return d.name;
            }));

        //make y axis to show bar names
        var yAxis = d3.svg.axis()
            .scale(y)
            //no tick marks
            .tickSize(0)
            .orient("left");

        var gy = svg.append("g")
            .attr("class", "y axis")
            .call(yAxis)

        var bars = svg.selectAll(".bar")
            .data(data)
            .enter()
            .append("g")

        //append rects
        bars.append("rect")
            .attr("class", "bar")
            .attr("y", function (d) {
                return y(d.name);
            })
            .attr("height", y.rangeBand())
            .attr("x", 0)
            .attr("width", function (d) {
                return x(d.value);
            });

        //add a value label to the right of each bar
        bars.append("text")
            .attr("class", "label")
            //y position of the label is halfway down the bar
            .attr("y", function (d) {
                return y(d.name) + y.rangeBand() / 2 + 4;
            })
            //x position is 3 pixels to the right of the bar
            .attr("x", function (d) {
                return x(d.value) + 3;
            })
            .text(function (d) {
                return Math.round(d.value * 100) + '%';
            });
    }

    function generateHalfDonut(percentage, elementId) {
        var value = percentage
        var text = Math.round(value * 100) + '%'
        var data = [value, 1 - value]

        // Settings
        var width = 300
        var height = 150
        var anglesRange = 0.5 * Math.PI
        var radis = Math.min(width, 2 * height) / 2
        var thickness = 50
        // Utility 
        //     var colors = d3.scale.category10();
        var colors = ["#5EBBF8", "#eaeaea"]

        var pies = d3.layout.pie()
            .value(d => d)
            .sort(null)
            .startAngle(anglesRange * -1)
            .endAngle(anglesRange)

        var arc = d3.svg.arc()
            .outerRadius(radis)
            .innerRadius(radis - thickness)

        var translation = (x, y) => `translate(${x}, ${y})`

        // Feel free to change or delete any of the code you see in this editor!
        var svg = d3.select("#" + elementId).append("svg")
            .attr("width", width)
            .attr("height", height)
            .attr("class", "half-donut")
            .append("g")
            .attr("transform", translation(width / 2, height))


        svg.selectAll("path")
            .data(pies(data))
            .enter()
            .append("path")
            .attr("fill", (d, i) => colors[i])
            .attr("d", arc)

        svg.append("text")
            .text(d => text)
            .attr("dy", "-3rem")
            .attr("class", "label")
            .attr("text-anchor", "middle")
    }

    $scope.GetEmployees = function () {
        $http.post("/Home/GetAirEmployees").then(function (response) {
            $scope.employees = response.data;
            console.log(response.data);
        });
    };

    $scope.GetPercentages = function () {
        $http.post("/Home/GetPercentages").then(function (response) {
            $scope.percentage = response.data;
            // $scope.percntage = {"lgtbt": 0.3, "military": 0.67}
            for (var i in $scope.percentage) {
                generateHalfDonut($scope.percentage[i], i);
            }
        });
    };

    $scope.GetNumbers = function () {
        $http.post("/Home/GetNumbers").then(function (response) {
            $scope.number = response.data;
            // $scope.number = {
            //  gender: [{ name: "male", value: 0.3 }, { name: "female", value: 0.5 }],
            //  ethnicity: [{ name: "hispanic", value: 0.3 }, { name: "black", value: 0.5 }]
            // }
            $timeout(function () {
                var keys = Object.keys($scope.number);
                for (var i = 0; i < keys.length; i++) {
                    //if (i == "age") {
                    //    generateBarChart($scope.number[i], i);
                    //}
                    generateBarChart($scope.number[keys[i]], keys[i]);
                }
            })
        });
    };

    //$scope.GetGenders = function () {
    //    $http.post("/Diversity/GetGenders").then(function (response) {
    //        console.log(response.data);
    //    });
    //}

    //$scope.GetLanguages = function () {
    //    $http.post("/Diversity/GetLanguages").then(function (response)){
    //        console.log(response.data);
    //    }
    //}

}])