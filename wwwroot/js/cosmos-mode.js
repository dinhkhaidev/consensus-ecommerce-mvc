(() => {
    "use strict";

    const validModes = new Set(["light", "dark", "cosmos"]);
    const threeModuleUrl = "https://cdn.jsdelivr.net/npm/three@0.160.0/build/three.module.js";
    const reduceMotionQuery = window.matchMedia("(prefers-reduced-motion: reduce)");

    let currentMode = null;
    let cosmosBackdrop = null;
    let cosmosFrame = 0;
    let cosmosState = null;
    let pendingThreeStart = null;
    let resizeHandler = null;
    let visibilityHandler = null;
    let pointerHandler = null;
    let introTimer = 0;
    let longPressTimer = 0;
    let ignoreNextClick = false;

    const pointer = {
        x: 0,
        y: 0,
        smoothX: 0,
        smoothY: 0
    };

    function getInitialMode() {
        const saved = localStorage.getItem("themeMode");
        if (validModes.has(saved)) {
            return saved;
        }

        return localStorage.getItem("darkMode") === "true" ? "dark" : "light";
    }

    function persistMode(mode) {
        localStorage.setItem("themeMode", mode);
        localStorage.setItem("darkMode", mode === "dark" || mode === "cosmos" ? "true" : "false");
    }

    function applyMode(mode, shouldPersist = true) {
        if (!validModes.has(mode)) {
            mode = "light";
        }

        const previousMode = currentMode;
        currentMode = mode;

        if (mode === "dark" || mode === "cosmos") {
            document.documentElement.setAttribute("data-bs-theme", "dark");
        } else {
            document.documentElement.removeAttribute("data-bs-theme");
        }

        if (mode === "cosmos") {
            document.documentElement.setAttribute("data-scene-mode", "cosmos");
            startCosmos({ runIntro: shouldPersist && previousMode !== "cosmos" });
        } else {
            document.documentElement.removeAttribute("data-scene-mode");
            stopCosmos();
        }

        if (shouldPersist) {
            persistMode(mode);
        }

        updateThemeControl(mode);
    }

    function updateThemeControl(mode) {
        const icon = document.getElementById("darkModeIcon");
        const toggle = document.getElementById("darkModeToggle");

        if (icon) {
            icon.className = "bi fs-5";
            if (mode === "cosmos") {
                icon.classList.add("bi-stars");
            } else if (mode === "dark") {
                icon.classList.add("bi-sun-fill");
            } else {
                icon.classList.add("bi-moon-stars");
            }
        }

        if (toggle) {
            toggle.setAttribute("aria-expanded", "false");
            toggle.setAttribute("aria-label", mode === "cosmos" ? "Cosmos mode" : "Toggle Dark Mode");
        }

        document.querySelectorAll("[data-theme-choice]").forEach(button => {
            const isActive = button.getAttribute("data-theme-choice") === mode;
            button.classList.toggle("is-active", isActive);
            button.setAttribute("aria-current", isActive ? "true" : "false");
        });
    }

    function openThemeMenu() {
        const menu = document.getElementById("themeModeMenu");
        const toggle = document.getElementById("darkModeToggle");
        if (!menu) {
            return;
        }

        menu.classList.add("is-open");
        toggle?.setAttribute("aria-expanded", "true");
    }

    function closeThemeMenu() {
        const menu = document.getElementById("themeModeMenu");
        const toggle = document.getElementById("darkModeToggle");
        if (!menu) {
            return;
        }

        menu.classList.remove("is-open");
        toggle?.setAttribute("aria-expanded", "false");
    }

    function isThemeMenuOpen() {
        return document.getElementById("themeModeMenu")?.classList.contains("is-open") === true;
    }

    function toggleLightDark() {
        applyMode(currentMode === "light" ? "dark" : "light");
    }

    function setupThemeControl() {
        const toggle = document.getElementById("darkModeToggle");
        const menu = document.getElementById("themeModeMenu");

        if (!toggle) {
            return;
        }

        const clearLongPress = () => {
            if (longPressTimer) {
                window.clearTimeout(longPressTimer);
                longPressTimer = 0;
            }
        };

        toggle.addEventListener("click", event => {
            if (ignoreNextClick) {
                ignoreNextClick = false;
                event.preventDefault();
                return;
            }

            if (event.shiftKey || event.altKey) {
                event.preventDefault();
                isThemeMenuOpen() ? closeThemeMenu() : openThemeMenu();
                return;
            }

            toggleLightDark();
        });

        toggle.addEventListener("contextmenu", event => {
            event.preventDefault();
            openThemeMenu();
        });

        toggle.addEventListener("pointerdown", event => {
            if (event.pointerType === "mouse" && event.button !== 0) {
                return;
            }

            clearLongPress();
            longPressTimer = window.setTimeout(() => {
                ignoreNextClick = true;
                openThemeMenu();
            }, 520);
        });

        ["pointerup", "pointercancel", "pointerleave"].forEach(eventName => {
            toggle.addEventListener(eventName, clearLongPress);
        });

        menu?.querySelectorAll("[data-theme-choice]").forEach(button => {
            button.addEventListener("click", () => {
                applyMode(button.getAttribute("data-theme-choice"));
                closeThemeMenu();
            });
        });

        document.addEventListener("click", event => {
            if (!menu || !isThemeMenuOpen()) {
                return;
            }

            if (!menu.contains(event.target) && !toggle.contains(event.target)) {
                closeThemeMenu();
            }
        });

        document.addEventListener("keydown", event => {
            if (event.key === "Escape") {
                closeThemeMenu();
            }
        });
    }

    function startCosmos(options = {}) {
        if (cosmosBackdrop) {
            if (options.runIntro) {
                runCosmosIntro();
            }
            return;
        }

        cosmosBackdrop = document.createElement("div");
        cosmosBackdrop.id = "cosmosBackdrop";
        cosmosBackdrop.className = "cosmos-backdrop";
        document.body.prepend(cosmosBackdrop);

        requestAnimationFrame(() => cosmosBackdrop?.classList.add("is-live"));
        if (options.runIntro) {
            runCosmosIntro();
        } else {
            cosmosBackdrop.classList.add("is-revealed");
            document.documentElement.removeAttribute("data-cosmos-web");
        }

        resizeHandler = () => resizeCosmos();
        visibilityHandler = () => {
            if (document.hidden) {
                cancelAnimationFrame(cosmosFrame);
                cosmosFrame = 0;
                return;
            }

            if (!cosmosFrame && cosmosState && !reduceMotionQuery.matches) {
                cosmosFrame = requestAnimationFrame(animateCosmos);
            }
        };
        pointerHandler = event => {
            pointer.x = (event.clientX / Math.max(window.innerWidth, 1) - 0.5) * 2;
            pointer.y = (event.clientY / Math.max(window.innerHeight, 1) - 0.5) * 2;
        };

        window.addEventListener("resize", resizeHandler);
        document.addEventListener("visibilitychange", visibilityHandler);
        window.addEventListener("pointermove", pointerHandler, { passive: true });

        pendingThreeStart = import(threeModuleUrl)
            .then(module => {
                if (!cosmosBackdrop || currentMode !== "cosmos") {
                    return;
                }

                setupThreeCosmos(module);
                resizeCosmos();
                renderCosmos(performance.now());

                if (!reduceMotionQuery.matches) {
                    cosmosFrame = requestAnimationFrame(animateCosmos);
                }
            })
            .catch(() => {
                if (cosmosBackdrop) {
                    cosmosBackdrop.classList.add("is-fallback");
                }
            });
    }

    function stopCosmos() {
        if (introTimer) {
            window.clearTimeout(introTimer);
            introTimer = 0;
        }

        if (cosmosFrame) {
            cancelAnimationFrame(cosmosFrame);
            cosmosFrame = 0;
        }

        if (resizeHandler) {
            window.removeEventListener("resize", resizeHandler);
            resizeHandler = null;
        }

        if (visibilityHandler) {
            document.removeEventListener("visibilitychange", visibilityHandler);
            visibilityHandler = null;
        }

        if (pointerHandler) {
            window.removeEventListener("pointermove", pointerHandler);
            pointerHandler = null;
        }

        disposeCosmosScene();
        pendingThreeStart = null;
        document.documentElement.removeAttribute("data-cosmos-web");

        if (cosmosBackdrop) {
            const oldBackdrop = cosmosBackdrop;
            oldBackdrop.classList.remove("is-live");
            window.setTimeout(() => oldBackdrop.remove(), 460);
        }

        cosmosBackdrop = null;
    }

    function runCosmosIntro() {
        if (!cosmosBackdrop) {
            return;
        }

        if (introTimer) {
            window.clearTimeout(introTimer);
            introTimer = 0;
        }

        cosmosBackdrop.classList.remove("is-revealed", "is-showcase", "is-revealing-web");
        document.documentElement.setAttribute("data-cosmos-web", "hidden");
        cosmosBackdrop.querySelector(".cosmos-intro")?.remove();

        if (reduceMotionQuery.matches) {
            cosmosBackdrop.classList.add("is-revealed");
            document.documentElement.removeAttribute("data-cosmos-web");
            return;
        }

        const intro = document.createElement("div");
        intro.className = "cosmos-intro";
        intro.setAttribute("aria-hidden", "true");
        intro.innerHTML = `
            <div class="cosmos-intro-starwash"></div>
            <div class="cosmos-intro-planet intro-planet-left"><span></span></div>
            <div class="cosmos-intro-planet intro-planet-right"><span></span></div>
            <div class="cosmos-intro-impact">
                <span class="impact-contact"></span>
                <span class="impact-core"></span>
                <span class="impact-wave impact-wave-one"></span>
                <span class="impact-wave impact-wave-two"></span>
            </div>
            <div class="cosmos-intro-dust"></div>
            <div class="cosmos-intro-caption">Đang vào mode đi cảnh...</div>
        `;

        const dust = intro.querySelector(".cosmos-intro-dust");
        for (let index = 0; index < 34; index += 1) {
            const particle = document.createElement("span");
            particle.style.setProperty("--angle", `${(360 / 34) * index + Math.random() * 12}deg`);
            particle.style.setProperty("--distance", `${32 + Math.random() * 42}vmin`);
            particle.style.setProperty("--delay", `${1.88 + Math.random() * 0.08}s`);
            particle.style.setProperty("--size", `${2 + Math.random() * 5}px`);
            dust.appendChild(particle);
        }

        cosmosBackdrop.appendChild(intro);
        cosmosBackdrop.classList.add("is-intro-running");

        introTimer = window.setTimeout(() => {
            cosmosBackdrop?.classList.add("is-showcase");
            intro.classList.add("is-ending");
            window.setTimeout(() => {
                intro.remove();
            }, 680);
            introTimer = window.setTimeout(() => {
                document.documentElement.setAttribute("data-cosmos-web", "revealing");
                cosmosBackdrop?.classList.add("is-revealing-web");
                cosmosBackdrop?.classList.remove("is-intro-running");
                introTimer = window.setTimeout(() => {
                    cosmosBackdrop?.classList.add("is-revealed");
                    cosmosBackdrop?.classList.remove("is-showcase", "is-revealing-web");
                    document.documentElement.removeAttribute("data-cosmos-web");
                    introTimer = 0;
                }, 1500);
            }, 1500);
        }, 3000);
    }

    function setupThreeCosmos(THREE) {
        const renderer = new THREE.WebGLRenderer({
            alpha: true,
            antialias: true,
            powerPreference: "high-performance"
        });
        renderer.setClearColor(0x000000, 0);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.75));
        renderer.domElement.className = "cosmos-webgl";
        cosmosBackdrop.prepend(renderer.domElement);
        cosmosBackdrop.classList.add("has-webgl");

        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(44, 1, 0.1, 80);
        camera.position.set(0, 0.12, 8.4);

        const root = new THREE.Group();
        scene.add(root);

        scene.add(new THREE.AmbientLight(0x405885, 1.15));

        const keyLight = new THREE.DirectionalLight(0xdfeaff, 2.2);
        keyLight.position.set(-4, 2.2, 5);
        scene.add(keyLight);

        const rimLight = new THREE.PointLight(0x8e6dff, 3.6, 18);
        rimLight.position.set(3.4, -2.2, 3.2);
        scene.add(rimLight);

        const planetGroup = new THREE.Group();
        planetGroup.position.set(2.38, -0.72, -1.35);
        planetGroup.rotation.set(-0.12, 0.18, -0.16);
        root.add(planetGroup);

        const planet = new THREE.Mesh(
            new THREE.SphereGeometry(1.18, 96, 96),
            new THREE.MeshStandardMaterial({
                map: createPlanetTexture(THREE),
                roughness: 0.62,
                metalness: 0.06,
                emissive: new THREE.Color(0x101a3a),
                emissiveIntensity: 0.22
            })
        );
        planetGroup.add(planet);

        const atmosphere = new THREE.Mesh(
            new THREE.SphereGeometry(1.205, 96, 96),
            new THREE.MeshBasicMaterial({
                color: 0x8fcbff,
                transparent: true,
                opacity: 0.16,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
        planetGroup.add(atmosphere);

        const ring = new THREE.Mesh(
            new THREE.TorusGeometry(1.72, 0.018, 12, 220),
            new THREE.MeshBasicMaterial({
                color: 0xd7e8ff,
                transparent: true,
                opacity: 0.68,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
        ring.rotation.set(1.26, 0.12, -0.4);
        planetGroup.add(ring);

        const secondRing = new THREE.Mesh(
            new THREE.TorusGeometry(1.98, 0.008, 10, 220),
            new THREE.MeshBasicMaterial({
                color: 0xae8cff,
                transparent: true,
                opacity: 0.48,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
        secondRing.rotation.copy(ring.rotation);
        secondRing.rotation.z -= 0.04;
        planetGroup.add(secondRing);

        const orbitGroup = new THREE.Group();
        orbitGroup.position.copy(planetGroup.position);
        root.add(orbitGroup);

        const moon = new THREE.Mesh(
            new THREE.SphereGeometry(0.16, 36, 36),
            new THREE.MeshStandardMaterial({
                color: 0xf5efe2,
                roughness: 0.5,
                emissive: 0x2d245c,
                emissiveIntensity: 0.18
            })
        );
        moon.position.set(2.25, 0.2, 0.1);
        orbitGroup.add(moon);

        const orbitLine = new THREE.Mesh(
            new THREE.TorusGeometry(2.25, 0.0035, 8, 180),
            new THREE.MeshBasicMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.24,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
        orbitLine.rotation.set(1.42, 0.28, 0.18);
        orbitGroup.add(orbitLine);

        const distantPlanet = new THREE.Mesh(
            new THREE.SphereGeometry(0.52, 64, 64),
            new THREE.MeshStandardMaterial({
                color: 0xffb56b,
                roughness: 0.66,
                metalness: 0.04,
                emissive: 0x5b2235,
                emissiveIntensity: 0.32
            })
        );
        distantPlanet.position.set(-3.25, 1.28, -4.9);
        root.add(distantPlanet);

        const distantRing = new THREE.Mesh(
            new THREE.TorusGeometry(0.82, 0.008, 8, 140),
            new THREE.MeshBasicMaterial({
                color: 0xffdfaa,
                transparent: true,
                opacity: 0.42,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
        distantRing.position.copy(distantPlanet.position);
        distantRing.rotation.set(1.2, 0.18, -0.72);
        root.add(distantRing);

        const icePlanet = new THREE.Mesh(
            new THREE.SphereGeometry(0.34, 48, 48),
            new THREE.MeshStandardMaterial({
                color: 0x92e4ff,
                roughness: 0.48,
                emissive: 0x123e66,
                emissiveIntensity: 0.42
            })
        );
        icePlanet.position.set(-2.25, -1.65, -3.4);
        root.add(icePlanet);

        const orbitalArc = new THREE.Mesh(
            new THREE.TorusGeometry(3.8, 0.004, 8, 260),
            new THREE.MeshBasicMaterial({
                color: 0xa9caff,
                transparent: true,
                opacity: 0.18,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
        orbitalArc.position.set(0.4, -0.05, -2.2);
        orbitalArc.rotation.set(1.36, -0.22, 0.28);
        root.add(orbitalArc);

        const meteorField = createMeteorField(THREE);
        meteorField.position.set(-0.4, 0.05, -1.4);
        root.add(meteorField);

        const deepStars = createStars(THREE, 1200, 42, 0.018);
        root.add(deepStars);

        const nearStars = createStars(THREE, 320, 17, 0.032);
        nearStars.position.z = 2;
        root.add(nearStars);

        const galaxy = createGalaxyDust(THREE);
        galaxy.position.set(-1.8, 0.18, -5);
        root.add(galaxy);

        cosmosState = {
            THREE,
            renderer,
            scene,
            camera,
            root,
            planet,
            atmosphere,
            ring,
            secondRing,
            orbitGroup,
            moon,
            distantPlanet,
            distantRing,
            icePlanet,
            orbitalArc,
            meteorField,
            deepStars,
            nearStars,
            galaxy
        };
    }

    function createPlanetTexture(THREE) {
        const canvas = document.createElement("canvas");
        canvas.width = 1024;
        canvas.height = 512;
        const ctx = canvas.getContext("2d");
        const gradient = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
        gradient.addColorStop(0, "#83d8ff");
        gradient.addColorStop(0.28, "#385ee8");
        gradient.addColorStop(0.58, "#16175f");
        gradient.addColorStop(1, "#08091a");
        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        for (let i = 0; i < 72; i += 1) {
            const y = Math.random() * canvas.height;
            const width = 180 + Math.random() * 560;
            const height = 12 + Math.random() * 38;
            const x = Math.random() * canvas.width - 120;
            ctx.globalAlpha = 0.1 + Math.random() * 0.18;
            ctx.fillStyle = i % 3 === 0 ? "#f0e6ff" : i % 2 === 0 ? "#71e9ff" : "#9b78ff";
            ctx.beginPath();
            ctx.ellipse(x, y, width, height, Math.random() * 0.18 - 0.09, 0, Math.PI * 2);
            ctx.fill();
        }

        ctx.globalAlpha = 0.32;
        ctx.fillStyle = "#ffffff";
        ctx.beginPath();
        ctx.ellipse(260, 148, 180, 34, -0.18, 0, Math.PI * 2);
        ctx.fill();

        const texture = new THREE.CanvasTexture(canvas);
        texture.colorSpace = THREE.SRGBColorSpace;
        texture.anisotropy = 4;
        return texture;
    }

    function createStars(THREE, count, spread, size) {
        const geometry = new THREE.BufferGeometry();
        const positions = new Float32Array(count * 3);
        const colors = new Float32Array(count * 3);
        const colorA = new THREE.Color(0xffffff);
        const colorB = new THREE.Color(0x9bd6ff);
        const colorC = new THREE.Color(0xc7a2ff);

        for (let i = 0; i < count; i += 1) {
            const radius = 5 + Math.random() * spread;
            const theta = Math.random() * Math.PI * 2;
            const phi = Math.acos(Math.random() * 2 - 1);
            positions[i * 3] = Math.sin(phi) * Math.cos(theta) * radius;
            positions[i * 3 + 1] = Math.sin(phi) * Math.sin(theta) * radius;
            positions[i * 3 + 2] = Math.cos(phi) * radius - 18;

            const color = i % 9 === 0 ? colorC : i % 5 === 0 ? colorB : colorA;
            colors[i * 3] = color.r;
            colors[i * 3 + 1] = color.g;
            colors[i * 3 + 2] = color.b;
        }

        geometry.setAttribute("position", new THREE.BufferAttribute(positions, 3));
        geometry.setAttribute("color", new THREE.BufferAttribute(colors, 3));

        return new THREE.Points(
            geometry,
            new THREE.PointsMaterial({
                size,
                sizeAttenuation: true,
                vertexColors: true,
                transparent: true,
                opacity: 0.86,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
    }

    function createGalaxyDust(THREE) {
        const count = 850;
        const geometry = new THREE.BufferGeometry();
        const positions = new Float32Array(count * 3);
        const colors = new Float32Array(count * 3);

        for (let i = 0; i < count; i += 1) {
            const arm = i % 3;
            const angle = (i / count) * Math.PI * 9 + arm * 2.1;
            const radius = Math.random() * 4.6;
            const jitter = (Math.random() - 0.5) * 0.72;
            positions[i * 3] = Math.cos(angle) * radius + jitter;
            positions[i * 3 + 1] = Math.sin(angle) * radius * 0.22 + (Math.random() - 0.5) * 0.55;
            positions[i * 3 + 2] = Math.sin(angle) * radius * 0.28 + (Math.random() - 0.5) * 0.8;

            colors[i * 3] = 0.48 + Math.random() * 0.4;
            colors[i * 3 + 1] = 0.58 + Math.random() * 0.28;
            colors[i * 3 + 2] = 1;
        }

        geometry.setAttribute("position", new THREE.BufferAttribute(positions, 3));
        geometry.setAttribute("color", new THREE.BufferAttribute(colors, 3));

        return new THREE.Points(
            geometry,
            new THREE.PointsMaterial({
                size: 0.036,
                sizeAttenuation: true,
                vertexColors: true,
                transparent: true,
                opacity: 0.42,
                blending: THREE.AdditiveBlending,
                depthWrite: false
            })
        );
    }

    function createMeteorField(THREE) {
        const field = new THREE.Group();
        const meteorGeometry = new THREE.ConeGeometry(0.018, 0.58, 8, 1, true);
        const meteorMaterial = new THREE.MeshBasicMaterial({
            color: 0xbfe9ff,
            transparent: true,
            opacity: 0.54,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });

        for (let index = 0; index < 34; index += 1) {
            const meteor = new THREE.Mesh(meteorGeometry, meteorMaterial);
            meteor.position.set(
                -5 + Math.random() * 10,
                -2.9 + Math.random() * 5.8,
                -7 + Math.random() * 8
            );
            meteor.rotation.set(Math.PI / 2.4, 0, -0.7 + Math.random() * 0.28);
            meteor.scale.setScalar(0.55 + Math.random() * 1.2);
            meteor.userData.speed = 0.0025 + Math.random() * 0.007;
            field.add(meteor);
        }

        return field;
    }

    function resizeCosmos() {
        if (!cosmosState) {
            return;
        }

        const width = window.innerWidth;
        const height = window.innerHeight;
        cosmosState.camera.aspect = width / Math.max(height, 1);
        cosmosState.camera.updateProjectionMatrix();
        cosmosState.renderer.setSize(width, height, false);
    }

    function animateCosmos(time) {
        renderCosmos(time);
        cosmosFrame = requestAnimationFrame(animateCosmos);
    }

    function renderCosmos(time) {
        if (!cosmosState) {
            return;
        }

        const t = time * 0.001;
        pointer.smoothX += (pointer.x - pointer.smoothX) * 0.035;
        pointer.smoothY += (pointer.y - pointer.smoothY) * 0.035;

        cosmosState.camera.position.x = pointer.smoothX * 0.34;
        cosmosState.camera.position.y = 0.12 - pointer.smoothY * 0.24;
        cosmosState.camera.lookAt(pointer.smoothX * 0.08, -0.04, -2.1);

        cosmosState.root.rotation.y = pointer.smoothX * 0.045;
        cosmosState.root.rotation.x = -pointer.smoothY * 0.03;

        cosmosState.planet.rotation.y = t * 0.16;
        cosmosState.planet.rotation.x = Math.sin(t * 0.35) * 0.025;
        cosmosState.atmosphere.rotation.y = -t * 0.08;
        cosmosState.ring.rotation.z = -0.4 + Math.sin(t * 0.42) * 0.028;
        cosmosState.secondRing.rotation.z = -0.44 + Math.cos(t * 0.36) * 0.024;
        cosmosState.orbitGroup.rotation.y = t * 0.28;
        cosmosState.orbitGroup.rotation.z = Math.sin(t * 0.22) * 0.18;
        cosmosState.deepStars.rotation.y = t * 0.006;
        cosmosState.nearStars.rotation.y = -t * 0.014;
        cosmosState.nearStars.rotation.x = Math.sin(t * 0.2) * 0.025;
        cosmosState.galaxy.rotation.z = t * 0.025;
        cosmosState.galaxy.rotation.y = -0.62 + Math.sin(t * 0.18) * 0.08;
        cosmosState.distantPlanet.rotation.y = -t * 0.11;
        cosmosState.distantRing.rotation.z = -0.72 + Math.sin(t * 0.28) * 0.06;
        cosmosState.icePlanet.rotation.y = t * 0.24;
        cosmosState.orbitalArc.rotation.z = 0.28 + Math.sin(t * 0.18) * 0.08;
        cosmosState.meteorField.children.forEach((meteor, index) => {
            meteor.position.x += meteor.userData.speed * 16;
            meteor.position.y -= meteor.userData.speed * 5;
            if (meteor.position.x > 5.7) {
                meteor.position.x = -5.9;
                meteor.position.y = -2.9 + ((index * 0.37 + t) % 5.8);
            }
        });

        cosmosState.renderer.render(cosmosState.scene, cosmosState.camera);
    }

    function disposeCosmosScene() {
        if (!cosmosState) {
            return;
        }

        const geometries = new Set();
        const materials = new Set();

        cosmosState.scene.traverse(item => {
            if (item.geometry) {
                geometries.add(item.geometry);
            }

            if (Array.isArray(item.material)) {
                item.material.forEach(material => materials.add(material));
            } else if (item.material) {
                materials.add(item.material);
            }
        });

        geometries.forEach(geometry => geometry.dispose?.());
        materials.forEach(disposeMaterial);
        cosmosState.renderer.dispose();
        cosmosState.renderer.domElement?.remove();
        cosmosState = null;
    }

    function disposeMaterial(material) {
        if (!material) {
            return;
        }

        Object.keys(material).forEach(key => {
            const value = material[key];
            if (value && typeof value.dispose === "function") {
                value.dispose();
            }
        });

        material.dispose?.();
    }

    document.addEventListener("DOMContentLoaded", () => {
        setupThemeControl();
        applyMode(getInitialMode(), false);
    });

    window.ConsensusTheme = {
        setMode: applyMode,
        getMode: () => currentMode || getInitialMode(),
        openMenu: openThemeMenu
    };
})();
