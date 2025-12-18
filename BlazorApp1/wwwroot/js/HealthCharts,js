window.healthCharts = (function () {
  const charts = {}; // store chart instances by canvasId

  function destroy(canvasId) {
    if (charts[canvasId]) {
      charts[canvasId].destroy();
      delete charts[canvasId];
    }
  }

  function renderLineChart(canvasId, title, labels, datasets) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    destroy(canvasId);

    charts[canvasId] = new Chart(ctx, {
      type: "line",
      data: {
        labels: labels,
        datasets: datasets
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: true },
          title: { display: true, text: title }
        },
        scales: {
          y: { beginAtZero: false }
        }
      }
    });
  }

  return {
    renderLineChart,
    destroy
  };
})();
