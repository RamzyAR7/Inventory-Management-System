///**
// * Radial Chart Initialization for Dashboard
// */
//function initRadialChart(elementId, percentage, color) {
//    // Check if the element exists
//    if (!document.getElementById(elementId)) {
//        console.error(`Element with ID ${elementId} not found.`);
//        return;
//    }

//    // Create options for ApexCharts radial chart
//    const options = {
//        series: [percentage],
//        chart: {
//            height: 120,
//            type: 'radialBar',
//        },
//        plotOptions: {
//            radialBar: {
//                hollow: {
//                    margin: 0,
//                    size: '65%'
//                },
//                track: {
//                    background: '#f2f2f2',
//                },
//                dataLabels: {
//                    name: {
//                        show: false
//                    },
//                    value: {
//                        show: true,
//                        fontSize: '14px',
//                        fontWeight: 'bold',
//                        formatter: function (val) {
//                            return val + '%';
//                        }
//                    }
//                }
//            }
//        },
//        colors: [color],
//        stroke: {
//            lineCap: "round",
//        }
//    };

//    // Initialize the chart
//    try {
//        const chart = new ApexCharts(document.getElementById(elementId), options);
//        chart.render();
//    } catch (error) {
//        console.error(`Error initializing chart (${elementId}):`, error);
//    }
//}

///**
// * Initialize inventory analytics chart - line chart showing inventory trends
// */
//function initInventoryAnalyticsChart(data) {
//    // If no data provided, use sample data
//    if (!data) {
//        data = {
//            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul'],
//            series: [
//                {
//                    name: 'Stock In',
//                    data: [65, 72, 62, 83, 85, 72, 92]
//                },
//                {
//                    name: 'Stock Out',
//                    data: [40, 58, 32, 58, 62, 53, 65]
//                }
//            ]
//        };
//    }

//    const options = {
//        series: data.series,
//        chart: {
//            height: 350,
//            type: 'line',
//            toolbar: {
//                show: false
//            },
//        },
//        colors: ['#34c38f', '#f46a6a'],
//        dataLabels: {
//            enabled: false,
//        },
//        stroke: {
//            curve: 'smooth',
//            width: 3,
//        },
//        xaxis: {
//            categories: data.categories,
//        },
//        grid: {
//            borderColor: '#f1f1f1',
//        },
//        markers: {
//            size: 4
//        },
//        legend: {
//            position: 'top',
//            horizontalAlign: 'right',
//        }
//    };

//    const chart = new ApexCharts(document.getElementById('inventoryAnalyticsChart'), options);
//    chart.render();
//}

//// Initialize analytics chart on document ready
//$(document).ready(function () {
//    // This would normally fetch data from the server
//    // For now, we'll use the default sample data
//    initInventoryAnalyticsChart();
//});

/**
 * Initialize a radial chart with the specified percentage
 * @param {string} elementId - The ID of the HTML element to render the chart in
 * @param {number} percentage - The percentage value to display (0-100)
 * @param {string} color - The color of the chart (hex code)
 */
function initRadialChart(elementId, percentage, color) {
    var options = {
        series: [percentage],
        chart: {
            height: 130,
            type: 'radialBar',
        },
        plotOptions: {
            radialBar: {
                hollow: {
                    size: '65%',
                },
                dataLabels: {
                    show: true,
                    name: {
                        show: true,
                        fontSize: '16px',
                        fontWeight: 600,
                        offsetY: -10,
                        color: color
                    },
                    value: {
                        show: false
                    }
                }
            }
        },
        labels: [percentage + '%'],
        colors: [color]
    };

    var chart = new ApexCharts(document.querySelector("#" + elementId), options);
    chart.render();
}