// LocalBuzz client-side interactions: Geolocation & forms

(function () {
  'use strict';

  // Check and initialize stored coordinates
  var storedLat = sessionStorage.getItem('lb_user_lat');
  var storedLng = sessionStorage.getItem('lb_user_lng');

  function updatePinButtons(active) {
    var pinBtns = document.querySelectorAll('.js-pin-btn');
    pinBtns.forEach(function (btn) {
      if (active) {
        btn.classList.add('active');
        btn.setAttribute('title', 'Location active (Navsari nearby)');
      } else {
        btn.classList.remove('active');
        btn.setAttribute('title', 'Use my current location');
      }
    });
  }

  function setCoords(lat, lng) {
    sessionStorage.setItem('lb_user_lat', lat);
    sessionStorage.setItem('lb_user_lng', lng);

    var latInputs = document.querySelectorAll('.js-lat-input');
    var lngInputs = document.querySelectorAll('.js-lng-input');

    latInputs.forEach(function (inp) { inp.value = lat; });
    lngInputs.forEach(function (inp) { inp.value = lng; });

    updatePinButtons(true);
  }

  function requestLocation(callback) {
    if (!navigator.geolocation) {
      if (callback) callback(null);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      function (pos) {
        var lat = pos.coords.latitude;
        var lng = pos.coords.longitude;
        setCoords(lat, lng);
        if (callback) callback({ lat: lat, lng: lng });
      },
      function (err) {
        console.warn('Geolocation denied or unavailable:', err.message);
        if (callback) callback(null);
      },
      { timeout: 5000, enableHighAccuracy: false }
    );
  }

  document.addEventListener('DOMContentLoaded', function () {
    // Populate hidden inputs if coords in sessionStorage
    if (storedLat && storedLng) {
      var latInputs = document.querySelectorAll('.js-lat-input');
      var lngInputs = document.querySelectorAll('.js-lng-input');
      latInputs.forEach(function (inp) {
        if (!inp.value) inp.value = storedLat;
      });
      lngInputs.forEach(function (inp) {
        if (!inp.value) inp.value = storedLng;
      });
      updatePinButtons(true);
    }

    // Pin button click
    var pinBtns = document.querySelectorAll('.js-pin-btn');
    pinBtns.forEach(function (btn) {
      btn.addEventListener('click', function (e) {
        e.preventDefault();
        requestLocation(function (coords) {
          if (coords) {
            // If on search page, we can optionally submit the form
            var form = btn.closest('form');
            if (form && btn.classList.contains('js-auto-submit')) {
              form.submit();
            }
          }
        });
      });
    });

    // Search form submission handler
    var searchForms = document.querySelectorAll('.js-search-form');
    searchForms.forEach(function (form) {
      form.addEventListener('submit', function (e) {
        var latInp = form.querySelector('.js-lat-input');
        var lngInp = form.querySelector('.js-lng-input');

        // If coordinates already present, let it submit directly
        if (latInp && latInp.value && lngInp && lngInp.value) {
          return true;
        }

        // Check sessionStorage
        var sLat = sessionStorage.getItem('lb_user_lat');
        var sLng = sessionStorage.getItem('lb_user_lng');
        if (sLat && sLng) {
          if (latInp) latInp.value = sLat;
          if (lngInp) lngInp.value = sLng;
          return true;
        }

        // Try once with 5s timeout if geolocation supported and user has not denied before
        if (navigator.geolocation && !form.dataset.geoAttempted) {
          e.preventDefault();
          form.dataset.geoAttempted = "true";
          requestLocation(function (coords) {
            if (coords) {
              if (latInp) latInp.value = coords.lat;
              if (lngInp) lngInp.value = coords.lng;
            }
            form.submit();
          });
        }
      });
    });

    // Dashboard shop coordinate autofill button
    var shopLocBtn = document.getElementById('btn-get-shop-coords');
    if (shopLocBtn) {
      shopLocBtn.addEventListener('click', function () {
        shopLocBtn.innerText = 'Detecting...';
        requestLocation(function (coords) {
          shopLocBtn.innerText = '📍 Use My Current Location';
          if (coords) {
            var latField = document.getElementById('shop-latitude');
            var lngField = document.getElementById('shop-longitude');
            if (latField) latField.value = coords.lat.toFixed(6);
            if (lngField) lngField.value = coords.lng.toFixed(6);
          } else {
            alert('Could not retrieve your location. Please check browser permissions or enter coordinates manually.');
          }
        });
      });
    }
  });
})();
