window.waitingRoomSound = {
  play: function () {
    const audio = new Audio("/sounds/ding-dong.mp3");
    audio.play().catch(() => {
      /* Autoplay-Block ignorieren */
    });
  },
};
