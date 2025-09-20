(function () {
  'use strict';

  /**
   * CORSなどでfetchが失敗する環境でも、ユーザー操作の文脈であれば
   * window.open にフォールバックして手動オープンできるようにする。
   */
  function buildUrl(pathname) {
    var baseInput = document.getElementById('baseUrlInput');
    var base = (baseInput && baseInput.value) ? baseInput.value.trim() : '';
    if (base === '') {
      return pathname;
    }
    try {
      var url = new URL(pathname, base);
      return url.toString();
    } catch (e) {
      return pathname;
    }
  }

  function setStatus(message, isError) {
    var el = document.getElementById('status');
    if (!el) return;
    el.textContent = message || '';
    if (isError) {
      el.classList.add('error');
    } else {
      el.classList.remove('error');
    }
  }

  async function callEndpoint(pathname) {
    var url = buildUrl(pathname);
    setStatus(pathname + ' 呼び出し中…');
    try {
      var res = await fetch(url, {
        method: 'GET',
        mode: 'cors',
        headers: {
          // ngrokのブラウザ警告(ERR_NGROK_6024)をスキップするための推奨ヘッダー
          'ngrok-skip-browser-warning': 'true'
        }
      });
      if (!res.ok) {
        setStatus('失敗: HTTP ' + res.status + ' ' + res.statusText, true);
        return;
      }
      var text = await res.text();
      setStatus('成功: ' + text);
    } catch (err) {
      // CORSなどでブロックされる場合は新規タブで開く
      setStatus('fetch失敗。新しいタブで開きます: ' + url, true);
      try {
        window.open(url, '_blank', 'noopener,noreferrer');
      } catch (_e) {}
    }
  }

  function onReady() {
    var attackBtn = document.getElementById('attackBtn');
    var defendBtn = document.getElementById('defendBtn');
    if (attackBtn) {
      attackBtn.addEventListener('click', function () { callEndpoint('/attack'); });
    }
    if (defendBtn) {
      defendBtn.addEventListener('click', function () { callEndpoint('/defend'); });
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', onReady);
  } else {
    onReady();
  }
})();


