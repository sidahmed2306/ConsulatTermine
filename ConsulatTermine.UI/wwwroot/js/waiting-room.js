(function () {
  // verhindert Sound beim allerersten Aufruf (initial load)
  let initialized = false;

  window.onWaitingRoomUpdated = function () {
    // Initialer Render: NICHT klingeln
    if (!initialized) {
      initialized = true;
      return;
    }

    // 🔔 Sound nur wenn freigeschaltet wurde
    if (
      window.SoundService &&
      typeof window.SoundService.playDingDong === "function"
    ) {
      window.SoundService.playDingDong();
    }

    // optional highlight
    const card = document.querySelector(".waiting-room-card");
    if (card) {
      card.classList.add("flash");
      setTimeout(() => card.classList.remove("flash"), 1500);
    }
  };
})();
