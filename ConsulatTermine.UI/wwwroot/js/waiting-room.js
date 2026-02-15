(function () {
  // Wird nur bei echtem Aufruf/Recall aus Blazor aufgerufen, nie beim ersten Seitenload.
  window.onWaitingRoomUpdated = function () {
    if (
      window.SoundService &&
      typeof window.SoundService.playDingDong === "function"
    ) {
      window.SoundService.playDingDong();
    }

    // optional highlight
    const card = document.querySelector(".tv-card");
    if (card) {
      card.classList.add("flash");
      setTimeout(function () {
        card.classList.remove("flash");
      }, 1500);
    }
  };
})();
