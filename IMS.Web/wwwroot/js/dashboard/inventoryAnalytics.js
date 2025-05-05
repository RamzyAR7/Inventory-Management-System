/**
 * Inventory Management System - Dashboard Analytics
 * This file contains functions for fetching and displaying inventory analytics
 */

document.addEventListener('DOMContentLoaded', function () {
    // Initialize dashboard data
    initializeDashboard();
});

/**
 * Initialize all dashboard components and data
 */
function initializeDashboard() {
    // You could fetch additional data here if needed
    // For now, we're using the data already passed from the server to the view

    // Update progress indicators based on values
    updateProgressIndicators();
}

/**
 * Update progress indicators for radial charts based on actual values
 */
function updateProgressIndicators() {
    // Get values from the page
    const totalProducts = parseInt(document.getElementById('spanTotalProducts').innerText);
    const totalOrders = parseInt(document.getElementById('spanTotalOrders').innerText);
    const totalInventory = parseInt(document.getElementById('spanTotalInventory').innerText);

    // Calculate percentages (these are example calculations - adjust based on your business logic)
    let productPercentage = Math.min(85, Math.max(10, calculatePercentage(totalProducts, 1000)));
    let orderPercentage = Math.min(85, Math.max(10, calculatePercentage(totalOrders, 500)));
    let inventoryPercentage = Math.min(85, Math.max(10, calculatePercentage(totalInventory, 5000)));

    // Initialize charts with calculated percentages
    initRadialChart('totalProductsRadialChart', productPercentage, '#34c38f');
    initRadialChart('totalOrdersRadialChart', orderPercentage, '#f1b44c');
    initRadialChart('totalInventoryRadialChart', inventoryPercentage, '#F0006B');
}

/**
 * Helper function to calculate percentage
 */
function calculatePercentage(value, target) {
    if (!value || !target) return 0;
    return Math.round((value / target) * 100);
}

/**
 * Fetch additional dashboard data if needed
 * This would typically call an API endpoint
 */
function fetchAdditionalDashboardData() {
    // Show loading spinners
    document.querySelectorAll('.chart-spinner').forEach(spinner => {
        spinner.style.display = 'block';
    });

    // Simulate API call - in production, replace with actual API call
    setTimeout(() => {
        // Hide spinners
        document.querySelectorAll('.chart-spinner').forEach(spinner => {
            spinner.style.display = 'none';
        });

        // Process the data (this is simulated)
        const sampleData = {
            categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul'],
            series: [
                {
                    name: 'Stock In',
                    data: [65, 72, 62, 83, 85, 72, 92]
                },
                {
                    name: 'Stock Out',
                    data: [40, 58, 32, 58, 62, 53, 65]
                }
            ]
        };

        // Update charts with fetched data
        initInventoryAnalyticsChart(sampleData);
    }, 1000);
}