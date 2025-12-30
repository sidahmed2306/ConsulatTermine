(function () {
  let audio = null;
  let unlocked = false;

  function getAudio() {
    if (audio) return audio;
    audio = new Audio("/sounds/ding-dong.mp3");
    audio.preload = "auto";
    audio.volume = 0.9;
    return audio;
  }

  window.SoundService = {
    // Muss 1x durch User-Klick aufgerufen werden (TV)
    unlock: async function () {
      try {
        if (unlocked) return true;

        const a = getAudio();
        a.muted = true;

        const p = a.play();
        if (p && p.catch) await p;

        a.pause();
        a.currentTime = 0;
        a.muted = false;

        unlocked = true;
        return true;
      } catch (e) {
        console.warn("Sound unlock failed:", e);
        return false;
      }
    },

    playDingDong: async function () {
      try {
        const a = getAudio();

        // Wenn noch nicht unlocked: versuchen (kann ohne User-Klick blocken)
        if (!unlocked) {
          await window.SoundService.unlock();
        }

        a.pause();
        a.currentTime = 0;

        const p = a.play();
        if (p && p.catch) await p;

        return true;
      } catch (e) {
        console.warn("playDingDong failed:", e);
        return false;
      }
    },
  };
})();
