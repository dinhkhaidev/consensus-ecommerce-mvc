import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js";
import { FBXLoader } from "three/addons/loaders/FBXLoader.js";

const currencyFormatter = new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND"
});

const MODEL_PATHS = {
    sofa: "/models/demo-products/sofa/sofa_02_1k.gltf",
    table: "/models/demo-products/coffee-table/modern_coffee_table_01_1k.gltf",
    chair: "/models/demo-products/lounge-chair/ArmChair_01_1k.gltf",
    plant: "/models/demo-products/plant/potted_plant_04_1k.gltf",
    cabinet: "/models/demo-products/tv-stand/drawer_cabinet_1k.gltf"
};

const SHARETEXTURES = {
    sofa22: asset("sofa-22", "sofa-22"),
    sofa21: asset("sofa-21", "sofa-21"),
    armchair16: asset("armchair-16", "armchair-16"),
    armchair15: asset("armchair-15", "armchair-15"),
    armchair14: asset("armchair-14", "armchair-14"),
    armchair13: asset("armchair-13", "armchair-13"),
    table5: asset("table-5", "table-5"),
    table4: asset("table-4", "table-4"),
    stool9: asset("stool-9", "stool-9")
};

const TEXTURE_SETS = {
    [SHARETEXTURES.sofa22.model]: textureSet("sofa-22", "3D_sofa_22", "2K"),
    [SHARETEXTURES.sofa21.model]: textureSet("sofa-21", "3D_sofa_21", "2K"),
    [SHARETEXTURES.armchair16.model]: textureSet("armchair-16", "3D_armchair_16", "2K"),
    [SHARETEXTURES.armchair15.model]: textureSet("armchair-15", "3D_armchair_14"),
    [SHARETEXTURES.armchair14.model]: textureSet("armchair-14", "3D_armchair_14"),
    [SHARETEXTURES.armchair13.model]: textureSet("armchair-13", "3D_armchair_13"),
    [SHARETEXTURES.table5.model]: textureSet("table-5", "picnic_table_01"),
    [SHARETEXTURES.table4.model]: textureSet("table-4", "Table_4"),
    [SHARETEXTURES.stool9.model]: textureSet("stool-9", "folding_camping_stool")
};

const STATIC_DECOR = [
    {
        id: "katana-wall",
        name: "Ceremonial Katana Wall Display",
        model3DUrl: "/models/rooms/decor/katana-wall.glb",
        model3DUrlCandidates: [
            "/models/rooms/decor/katana-wall.glb",
            "/models/rooms/decor/katana-wall/scene.gltf"
        ],
        fallback: "katana",
        position: [-3.82, 1.72, 0],
        rotation: [0, 90, 0],
        modelPreRotation: [0, 0, 90],
        maxHeight: 0.95,
        maxWidth: 3.8,
        credit: "Antique Katana 01 by Poly Haven (CC0)"
    }
];

const demoProducts = [
    withModelCandidates(
        product("dragon-01", "Shenron Guardian Dragon", "dragon", 20000000000, "/models/rooms/decor/shenron-yanez.glb", null, 1, 0, 18, "signature", "10k", "#58d6a0", "#d8c782"),
        ["/models/rooms/decor/shenron-yanez.glb", "/models/rooms/decor/shenron-yanez/scene.gltf"]
    ),
    product("sofa-01", "Signature Modular Sofa", "sofa", 16900000, SHARETEXTURES.sofa22.model, SHARETEXTURES.sofa22.thumb, 1.02, 0, 0, "hero", "8k-10k feel", "#d8b48c", "#6e4a32"),
    product("sofa-02", "Brown Triple Sofa", "sofa", 14800000, SHARETEXTURES.sofa21.model, SHARETEXTURES.sofa21.thumb, 1, 0, -8, "hero", "8k-10k feel", "#e6d4c0", "#5c4636"),
    product("sofa-03", "Compact Studio Sofa", "sofa", 9800000, MODEL_PATHS.sofa, null, 0.82, 0, 10, "standard", "7k", "#c7b7a1", "#4d3928"),
    product("table-01", "Wood Picnic Table", "table", 4200000, SHARETEXTURES.table5.model, SHARETEXTURES.table5.thumb, 1, 0, 0, "standard", "3k", "#c69c6d", "#2d2219"),
    product("table-02", "Industrial Kitchen Table", "table", 4800000, SHARETEXTURES.table4.model, SHARETEXTURES.table4.thumb, 1, 0, 20, "standard", "3k", "#b27d4e", "#2c2018"),
    product("table-03", "Black Frame Coffee Table", "table", 3600000, MODEL_PATHS.table, null, 1.05, 0, -12, "standard", "4k", "#7a6a5a", "#181513"),
    product("chair-01", "Grey Lounge Armchair", "chair", 6200000, SHARETEXTURES.armchair16.model, SHARETEXTURES.armchair16.thumb, 1, 0, 0, "hero", "8k-10k feel", "#bfc7b2", "#37402f"),
    product("chair-02", "Soft Grey Armchair", "chair", 5800000, SHARETEXTURES.armchair15.model, SHARETEXTURES.armchair15.thumb, 1, 0, 18, "standard", "3k", "#d7cab8", "#5a4b3d"),
    product("chair-03", "Mustard Accent Chair", "chair", 6500000, SHARETEXTURES.armchair14.model, SHARETEXTURES.armchair14.thumb, 1, 0, -14, "standard", "3k", "#aeb7aa", "#303b34"),
    product("chair-04", "Yellow Statement Chair", "chair", 6100000, SHARETEXTURES.armchair13.model, SHARETEXTURES.armchair13.thumb, 1, 0, 12, "standard", "3k", "#d5b45f", "#49381f"),
    product("stool-01", "Soft Ottoman Stool", "chair", 2100000, SHARETEXTURES.stool9.model, SHARETEXTURES.stool9.thumb, 1, 0, 0, "standard", "2k", "#c9aa84", "#5c412b"),
    product("lamp-01", "Warm Floor Lamp", "lamp", 1800000, null, 1, 0, 0, "fallback", "2k", "#f2c16d", "#4d3823"),
    product("lamp-02", "Arc Reading Lamp", "lamp", 2400000, null, 1, 0, 0, "fallback", "2k", "#e9bb73", "#3f3328"),
    product("lamp-03", "Slim Studio Lamp", "lamp", 1600000, null, 0.88, 0, 0, "fallback", "2k", "#f0d3a1", "#403022"),
    product("plant-01", "Decor Plant", "plant", 950000, MODEL_PATHS.plant, 1, 0, 0, "standard", "3k", "#7ca36f", "#254b2b"),
    product("plant-02", "Tall Corner Plant", "plant", 1350000, MODEL_PATHS.plant, 1.25, 0, -10, "standard", "3k", "#6f9b66", "#243d24"),
    product("plant-03", "Mini Table Plant", "plant", 650000, MODEL_PATHS.plant, 0.65, 0, 14, "standard", "3k", "#8bb67d", "#2f4c2d"),
    product("rug-01", "Soft Area Rug", "rug", 2200000, null, 1, 0, 0, "fallback", "1k", "#d8a36f", "#9d5d30"),
    product("rug-02", "Neutral Woven Rug", "rug", 2600000, null, 1.15, 0, 0, "fallback", "1k", "#c9b99f", "#7d674d"),
    product("rug-03", "Signature Clay Rug", "rug", 3100000, null, 0.95, 0, 0, "fallback", "1k", "#c77f55", "#6d3e2a"),
    product("cabinet-01", "Low Media Cabinet", "cabinet", 6400000, MODEL_PATHS.cabinet, 1, 0, 0, "standard", "5k", "#9f7b55", "#2c2118"),
    product("cabinet-02", "Walnut Storage Console", "cabinet", 7600000, MODEL_PATHS.cabinet, 1.14, 0, 0, "standard", "5k", "#8d6847", "#241b15"),
    product("cabinet-03", "Compact Sideboard", "cabinet", 5900000, MODEL_PATHS.cabinet, 0.84, 0, 0, "standard", "5k", "#a77d52", "#312319"),
    product("decor-01", "Gallery Wall Frame", "decor", 890000, null, 1, 0, 0, "fallback", "1k", "#ddc8a6", "#3a2b20"),
    product("decor-02", "Ceramic Vase Set", "decor", 1200000, null, 1, 0, 0, "fallback", "1k", "#e3ded4", "#5a4a3e"),
    product("decor-03", "Sculpture Accent", "decor", 1850000, null, 1, 0, 0, "fallback", "2k", "#b7aea0", "#29241f"),
    product("shelf-01", "Open Display Shelf", "shelf", 4300000, null, 1, 0, 0, "fallback", "3k", "#9c7350", "#251b14"),
    product("shelf-02", "Slim Book Shelf", "shelf", 5100000, null, 0.95, 0, 0, "fallback", "3k", "#8b6a4f", "#2c221a")
];

const state = {
    scene: null,
    camera: null,
    renderer: null,
    orbitControls: null,
    gltfLoader: null,
    fbxLoader: null,
    textureLoader: null,
    raycaster: new THREE.Raycaster(),
    pointer: new THREE.Vector2(),
    dragPlane: new THREE.Plane(new THREE.Vector3(0, 1, 0), 0),
    dragPoint: new THREE.Vector3(),
    dragOffset: new THREE.Vector3(),
    roomItems: [],
    selectedObject: null,
    selectionBox: null,
    selectionHalo: null,
    isDraggingObject: false,
    activePointerId: null,
    activeCategory: "all",
    searchQuery: "",
    modelCache: new Map(),
    cameraAnimation: null,
    detailViewer: null,
    detailProduct: null,
    animationFrame: null
};

const dom = {
    page: document.getElementById("room3dPage"),
    canvas: document.getElementById("room3dCanvas"),
    productList: document.getElementById("productList"),
    categoryFilters: document.getElementById("categoryFilters"),
    productSearchInput: document.getElementById("productSearchInput"),
    libraryCount: document.getElementById("libraryCount"),
    loadingOverlay: document.getElementById("loadingOverlay"),
    selectedInfo: document.getElementById("selectedInfo"),
    totalPrice: document.getElementById("totalPrice"),
    rotateLeftBtn: document.getElementById("rotateLeftBtn"),
    rotateRightBtn: document.getElementById("rotateRightBtn"),
    pitchUpBtn: document.getElementById("pitchUpBtn"),
    pitchDownBtn: document.getElementById("pitchDownBtn"),
    rollLeftBtn: document.getElementById("rollLeftBtn"),
    rollRightBtn: document.getElementById("rollRightBtn"),
    focusSelectedBtn: document.getElementById("focusSelectedBtn"),
    resetRotationBtn: document.getElementById("resetRotationBtn"),
    deleteBtn: document.getElementById("deleteBtn"),
    snapshotBtn: document.getElementById("snapshotBtn"),
    buyRoomBtn: document.getElementById("buyRoomBtn"),
    resetCameraBtn: document.getElementById("resetCameraBtn"),
    fullscreenBtn: document.getElementById("fullscreenBtn"),
    libraryToggleBtn: document.getElementById("libraryToggleBtn"),
    libraryCloseBtn: document.getElementById("libraryCloseBtn"),
    inspectorToggleBtn: document.getElementById("inspectorToggleBtn"),
    inspectorCloseBtn: document.getElementById("inspectorCloseBtn"),
    detailModal: document.getElementById("productDetailModal"),
    detailCanvas: document.getElementById("productDetailCanvas"),
    detailLoading: document.getElementById("detailLoading"),
    detailProductName: document.getElementById("detailProductName"),
    detailProductMeta: document.getElementById("detailProductMeta"),
    detailCloseBtn: document.getElementById("detailCloseBtn"),
    detailAddBtn: document.getElementById("detailAddBtn"),
    detailAutoRotateBtn: document.getElementById("detailAutoRotateBtn"),
    toast: document.getElementById("roomToast")
};

initRoom3D();

function initRoom3D() {
    if (!dom.canvas) return;

    createScene();
    createCamera();
    createRenderer();
    createLights();
    createRoomShell();
    createControls();
    renderCategoryFilters();
    renderProductList();
    bindEvents();
    updateInspector();
    updateTotal();
    handleResize();
    animate();

    window.setTimeout(() => dom.loadingOverlay?.classList.add("is-hidden"), 550);
}

function createScene() {
    state.scene = new THREE.Scene();
    state.scene.background = new THREE.Color(0xf2ece4);
    state.scene.fog = new THREE.Fog(0xf2ece4, 12, 24);
    state.gltfLoader = new GLTFLoader();
    state.fbxLoader = new FBXLoader();
    state.textureLoader = new THREE.TextureLoader();
}

function createCamera() {
    state.camera = new THREE.PerspectiveCamera(43, 1, 0.1, 100);
    state.camera.position.set(5, 3, 6);
    state.camera.lookAt(0, 1, 0);
}

function createRenderer() {
    state.renderer = new THREE.WebGLRenderer({
        canvas: dom.canvas,
        antialias: true,
        alpha: false,
        preserveDrawingBuffer: true
    });
    state.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    state.renderer.shadowMap.enabled = true;
    state.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    state.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    state.renderer.toneMappingExposure = 1.05;
    state.renderer.outputColorSpace = THREE.SRGBColorSpace;
}

function createLights() {
    const hemisphere = new THREE.HemisphereLight(0xfff4e7, 0x5f6268, 2.25);
    state.scene.add(hemisphere);

    const keyLight = new THREE.DirectionalLight(0xffefd0, 3.7);
    keyLight.position.set(4.8, 6.8, 4.4);
    keyLight.castShadow = true;
    keyLight.shadow.mapSize.set(2048, 2048);
    keyLight.shadow.camera.near = 0.5;
    keyLight.shadow.camera.far = 20;
    keyLight.shadow.camera.left = -7;
    keyLight.shadow.camera.right = 7;
    keyLight.shadow.camera.top = 7;
    keyLight.shadow.camera.bottom = -7;
    keyLight.shadow.bias = -0.00015;
    state.scene.add(keyLight);

    const windowGlow = new THREE.PointLight(0xffc17d, 1.15, 7, 2);
    windowGlow.position.set(-2.9, 2.35, -2.2);
    state.scene.add(windowGlow);

    const muralWash = new THREE.SpotLight(0xffd9a7, 1.1, 7, Math.PI / 5, 0.5, 1.5);
    muralWash.position.set(1.4, 3.1, 1.7);
    muralWash.target.position.set(0.7, 1.85, -3.9);
    state.scene.add(muralWash, muralWash.target);
}

function createControls() {
    state.orbitControls = new OrbitControls(state.camera, state.renderer.domElement);
    state.orbitControls.enableDamping = true;
    state.orbitControls.dampingFactor = 0.06;
    state.orbitControls.target.set(0, 1, 0);
    state.orbitControls.minDistance = 3;
    state.orbitControls.maxDistance = 12;
    state.orbitControls.maxPolarAngle = Math.PI / 2.05;
    state.orbitControls.enablePan = true;
}

function createRoomShell() {
    const floorMat = new THREE.MeshStandardMaterial({
        color: 0xb99266,
        roughness: 0.76,
        metalness: 0.02
    });
    const wallMat = new THREE.MeshStandardMaterial({
        color: 0xeee6dc,
        roughness: 0.9
    });
    const trimMat = new THREE.MeshStandardMaterial({
        color: 0x4a3425,
        roughness: 0.68
    });

    const floor = createBox(8, 0.08, 8, floorMat, [0, -0.04, 0]);
    floor.receiveShadow = true;
    state.scene.add(floor);

    addFloorLines();

    const backWall = createBox(8, 3.25, 0.12, wallMat, [0, 1.6, -4]);
    const leftWall = createBox(0.12, 3.25, 8, wallMat, [-4, 1.6, 0]);
    const rightNib = createBox(0.12, 3.25, 2.4, wallMat, [4, 1.6, -2.8]);
    [backWall, leftWall, rightNib].forEach(wall => {
        wall.receiveShadow = true;
        state.scene.add(wall);
    });

    createWindow(trimMat);
    createSignatureGraffiti(trimMat);
    createBuiltInRug();
    createFixedDecor();
    createSignatureDecor();
}

function addFloorLines() {
    const lineMat = new THREE.MeshBasicMaterial({ color: 0x8f6a45, transparent: true, opacity: 0.16 });
    for (let z = -3.5; z <= 3.5; z += 0.5) {
        state.scene.add(createBox(8, 0.006, 0.012, lineMat, [0, 0.006, z]));
    }
}

function createWindow(trimMat) {
    const glassMat = new THREE.MeshStandardMaterial({
        color: 0xdce9ee,
        roughness: 0.2,
        transparent: true,
        opacity: 0.62
    });

    const glass = createBox(1.75, 1.18, 0.025, glassMat, [-2.85, 1.95, -3.92]);
    const framePieces = [
        createBox(1.95, 0.06, 0.08, trimMat, [-2.85, 2.58, -3.86]),
        createBox(1.95, 0.06, 0.08, trimMat, [-2.85, 1.32, -3.86]),
        createBox(0.06, 1.28, 0.08, trimMat, [-3.83, 1.95, -3.86]),
        createBox(0.06, 1.28, 0.08, trimMat, [-1.87, 1.95, -3.86]),
        createBox(0.05, 1.18, 0.08, trimMat, [-2.85, 1.95, -3.85])
    ];
    state.scene.add(glass, ...framePieces);
}

function createSignatureGraffiti(trimMat) {
    const frameBackMat = new THREE.MeshStandardMaterial({ color: 0x16120f, roughness: 0.62 });
    const backingMat = new THREE.MeshStandardMaterial({ color: 0xfaf4ec, roughness: 0.74 });
    const centerX = 0.98;
    const centerY = 1.82;
    const artWidth = 2.74;
    const artHeight = 2.06;
    const backing = createBox(artWidth + 0.2, artHeight + 0.2, 0.035, backingMat, [centerX, centerY, -3.915]);
    const frame = [
        createBox(artWidth + 0.32, 0.055, 0.07, frameBackMat, [centerX, centerY + artHeight / 2 + 0.1, -3.875]),
        createBox(artWidth + 0.32, 0.055, 0.07, frameBackMat, [centerX, centerY - artHeight / 2 - 0.1, -3.875]),
        createBox(0.055, artHeight + 0.26, 0.07, frameBackMat, [centerX - artWidth / 2 - 0.1, centerY, -3.875]),
        createBox(0.055, artHeight + 0.26, 0.07, frameBackMat, [centerX + artWidth / 2 + 0.1, centerY, -3.875])
    ];
    state.scene.add(backing, ...frame);

    const geometry = new THREE.PlaneGeometry(artWidth, artHeight);
    const fallbackMaterial = new THREE.MeshStandardMaterial({
        color: 0xf8efe4,
        roughness: 0.72
    });
    const mural = new THREE.Mesh(geometry, fallbackMaterial);
    mural.position.set(centerX, centerY, -3.852);
    mural.castShadow = false;
    mural.receiveShadow = false;
    mural.name = "Khải Đẹp Trai signature graffiti";
    state.scene.add(mural);

    state.textureLoader.load(
        "/models/graffiti-3droom-mvc.svg",
        texture => {
            texture.colorSpace = THREE.SRGBColorSpace;
            texture.anisotropy = Math.min(8, state.renderer.capabilities.getMaxAnisotropy());
            mural.material.dispose();
            mural.material = new THREE.MeshStandardMaterial({
                map: texture,
                transparent: true,
                roughness: 0.62,
                metalness: 0.02
            });
        },
        undefined,
        () => {
            mural.material.dispose();
            mural.material = createCanvasGraffitiMaterial();
        }
    );

    const led = createBox(artWidth + 0.04, 0.025, 0.035, trimMat, [centerX, centerY + artHeight / 2 + 0.22, -3.82]);
    const glow = new THREE.PointLight(0xffc579, 0.8, 4.4, 2);
    glow.position.set(centerX, centerY + 0.55, -3.25);
    state.scene.add(led, glow);
}

function createCanvasGraffitiMaterial() {
    const canvas = document.createElement("canvas");
    canvas.width = 1024;
    canvas.height = 1024;
    const ctx = canvas.getContext("2d");
    const gradient = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
    gradient.addColorStop(0, "#ffcf5f");
    gradient.addColorStop(0.45, "#ff4f87");
    gradient.addColorStop(1, "#35d4ff");
    ctx.fillStyle = "#f8efe4";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.font = "900 108px Arial, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.lineWidth = 24;
    ctx.strokeStyle = "#111";
    ctx.strokeText("Khải Đẹp Trai", 512, 512);
    ctx.lineWidth = 10;
    ctx.strokeStyle = "#fff";
    ctx.strokeText("Khải Đẹp Trai", 512, 512);
    ctx.fillStyle = gradient;
    ctx.fillText("Khải Đẹp Trai", 512, 512);
    const texture = new THREE.CanvasTexture(canvas);
    texture.colorSpace = THREE.SRGBColorSpace;
    return new THREE.MeshStandardMaterial({ map: texture, roughness: 0.64 });
}

function createBuiltInRug() {
    const rugMat = new THREE.MeshStandardMaterial({
        color: 0xb97848,
        roughness: 0.94,
        metalness: 0.01
    });
    const rug = createBox(3.25, 0.035, 2.25, rugMat, [0.8, 0.02, 0.82]);
    rug.receiveShadow = true;
    state.scene.add(rug);
}

function createFixedDecor() {
    const plant = createPlantPlaceholder({ id: "fixed-plant", name: "Showroom plant", category: "plant", price: 0 }, true);
    plant.position.set(3.2, 0, -3.1);
    plant.scale.setScalar(0.9);

    state.scene.add(plant);
}

function createSignatureDecor() {
    STATIC_DECOR.forEach(config => {
        loadStaticDecorModel(config)
            .then(object => {
                placeStaticDecor(object, config);
                if (config.fallback === "katana") {
                    state.scene.add(createKatanaWallMount(config));
                }
                state.scene.add(object);
            })
            .catch(error => {
                console.warn(`Room3D static decor fallback for ${config.id}. Real model is missing or failed to load. Put the GLB/scene.gltf asset in /models/rooms/decor to replace this styled fallback.`, error);
                const fallback = createStaticDecorFallback(config);
                placeStaticDecor(fallback, config);
                state.scene.add(fallback);
            });
    });

    const katanaRim = new THREE.PointLight(0xffe5b6, 0.5, 2.8, 2);
    katanaRim.position.set(-3.18, 2.1, 0);
    state.scene.add(katanaRim);

    const katanaAccent = new THREE.SpotLight(0xffd7a0, 0.92, 3.6, Math.PI / 8, 0.52, 1.7);
    katanaAccent.position.set(-2.72, 2.36, 0);
    katanaAccent.target.position.set(-3.82, 1.82, 0);
    katanaAccent.castShadow = true;
    state.scene.add(katanaAccent, katanaAccent.target);
}

function loadStaticDecorModel(config) {
    const candidates = config.model3DUrlCandidates?.length
        ? config.model3DUrlCandidates
        : [config.model3DUrl].filter(Boolean);

    if (!candidates.length) {
        return Promise.reject(new Error("No static decor model URL configured."));
    }

    return loadStaticDecorCandidate(config, candidates, 0);
}

function loadStaticDecorCandidate(config, candidates, index) {
    return new Promise((resolve, reject) => {
        const modelUrl = candidates[index];
        state.gltfLoader.load(
            modelUrl,
            gltf => {
                const group = new THREE.Group();
                if (config.modelPreRotation) {
                    gltf.scene.rotation.set(
                        THREE.MathUtils.degToRad(config.modelPreRotation[0] || 0),
                        THREE.MathUtils.degToRad(config.modelPreRotation[1] || 0),
                        THREE.MathUtils.degToRad(config.modelPreRotation[2] || 0)
                    );
                }
                group.add(gltf.scene);
                prepareObjectMeshes(group);
                normalizeStaticDecorModel(group, config);
                applyStaticDecorMeta(group, config, false);
                group.userData.sourceUrl = modelUrl;
                resolve(group);
            },
            undefined,
            error => {
                const nextIndex = index + 1;
                if (nextIndex < candidates.length) {
                    loadStaticDecorCandidate(config, candidates, nextIndex).then(resolve).catch(reject);
                    return;
                }
                reject(error);
            }
        );
    });
}

function normalizeStaticDecorModel(group, config) {
    group.position.set(0, 0, 0);
    group.rotation.set(0, 0, 0);
    group.scale.set(1, 1, 1);
    group.updateMatrixWorld(true);

    const box = new THREE.Box3().setFromObject(group);
    if (box.isEmpty()) return;

    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());
    const maxWidth = Math.max(size.x, size.z) || 1;
    const heightScale = config.maxHeight ? config.maxHeight / Math.max(size.y, 0.001) : Infinity;
    const widthScale = config.maxWidth ? config.maxWidth / Math.max(maxWidth, 0.001) : Infinity;
    const fitScale = Math.min(heightScale, widthScale);
    const content = group.children[0] || group;

    content.position.x -= center.x;
    content.position.y -= box.min.y;
    content.position.z -= center.z;
    group.scale.setScalar(fitScale);
}

function placeStaticDecor(object, config) {
    object.position.set(config.position[0], config.position[1], config.position[2]);
    object.rotation.set(
        THREE.MathUtils.degToRad(config.rotation?.[0] || 0),
        THREE.MathUtils.degToRad(config.rotation?.[1] || 0),
        THREE.MathUtils.degToRad(config.rotation?.[2] || 0)
    );
    object.name = config.name;
    object.userData.baseScale = object.scale.x || 1;
    object.userData.decorFloatSeed = Math.random() * Math.PI * 2;
}

function applyStaticDecorMeta(group, config, isPlaceholder) {
    group.userData = {
        ...group.userData,
        type: "room-decor",
        id: config.id,
        name: config.name,
        fixed: true,
        isPlaceholder,
        credit: config.credit
    };
}

function createStaticDecorFallback(config) {
    const fallback = config.fallback === "katana"
        ? createKatanaFallback()
        : createShenronFallback();
    normalizeStaticDecorModel(fallback, config);
    applyStaticDecorMeta(fallback, config, true);
    return fallback;
}

function createKatanaWallMount(config) {
    const group = new THREE.Group();
    const alcoveMat = new THREE.MeshStandardMaterial({ color: 0x15100d, roughness: 0.86, metalness: 0.02 });
    const plaqueMat = new THREE.MeshStandardMaterial({ color: 0x241711, roughness: 0.78, metalness: 0.04 });
    const trimMat = new THREE.MeshStandardMaterial({ color: 0x6f4b2c, roughness: 0.64, metalness: 0.07 });
    const goldMat = new THREE.MeshStandardMaterial({ color: 0xc49a44, roughness: 0.36, metalness: 0.42 });
    const shadowMat = new THREE.MeshStandardMaterial({ color: 0x120d0a, roughness: 0.82 });
    const redMat = new THREE.MeshStandardMaterial({ color: 0x5f1513, roughness: 0.74, metalness: 0.03 });

    group.add(createBox(4.28, 1.06, 0.065, alcoveMat, [0, 0.16, -0.07]));
    group.add(createBox(3.88, 0.74, 0.052, plaqueMat, [0, 0.15, -0.02]));
    group.add(createBox(4.02, 0.06, 0.072, trimMat, [0, 0.55, 0.01]));
    group.add(createBox(4.02, 0.06, 0.072, trimMat, [0, -0.25, 0.01]));
    group.add(createBox(0.06, 0.82, 0.072, trimMat, [-2.04, 0.15, 0.01]));
    group.add(createBox(0.06, 0.82, 0.072, trimMat, [2.04, 0.15, 0.01]));
    group.add(createBox(3.34, 0.024, 0.058, goldMat, [0, 0.42, 0.035]));
    group.add(createBox(3.34, 0.024, 0.058, goldMat, [0, -0.1, 0.035]));
    group.add(createBox(0.1, 0.5, 0.04, redMat, [-1.78, 0.16, 0.04]));
    group.add(createBox(0.1, 0.5, 0.04, redMat, [1.78, 0.16, 0.04]));

    [-1.38, 1.38].forEach(x => {
        const support = createBox(0.13, 0.48, 0.13, shadowMat, [x, 0.08, 0.065]);
        const topPeg = createCylinder(0.04, 0.04, 0.24, goldMat, [x, 0.25, 0.15]);
        const lowerPeg = createCylinder(0.04, 0.04, 0.24, goldMat, [x, -0.08, 0.15]);
        topPeg.rotation.x = Math.PI / 2;
        lowerPeg.rotation.x = Math.PI / 2;
        group.add(support, topPeg, lowerPeg);
    });

    group.position.set(config.position[0] - 0.11, config.position[1] + 0.18, config.position[2]);
    group.rotation.set(
        THREE.MathUtils.degToRad(config.rotation?.[0] || 0),
        THREE.MathUtils.degToRad(config.rotation?.[1] || 0),
        0
    );
    group.name = "Katana wall rack";
    return group;
}

function createShenronFallback() {
    const group = new THREE.Group();
    const jade = new THREE.MeshStandardMaterial({
        color: 0x58d6a0,
        roughness: 0.5,
        metalness: 0.12,
        emissive: 0x176145,
        emissiveIntensity: 0.045
    });
    const belly = new THREE.MeshStandardMaterial({ color: 0xd8c782, roughness: 0.62, metalness: 0.08 });
    const gold = new THREE.MeshStandardMaterial({ color: 0xd4a93b, roughness: 0.5, metalness: 0.34 });
    const clawMat = new THREE.MeshStandardMaterial({ color: 0xd6c9a4, roughness: 0.54, metalness: 0.16 });

    const points = [];
    const turns = 0.96;
    for (let i = 0; i <= 128; i++) {
        const t = i / 128;
        const angle = -Math.PI * 0.62 + t * Math.PI * 2 * turns;
        const radiusX = 0.86 + Math.sin(t * Math.PI * 2.4) * 0.025;
        const radiusY = 0.95 + Math.cos(t * Math.PI * 1.8) * 0.03;
        const y = 1.16 + Math.sin(angle) * radiusY;
        const x = Math.cos(angle) * radiusX;
        const z = 0.11 + Math.sin(t * Math.PI * 4.2) * 0.12;
        points.push(new THREE.Vector3(x, y, z));
    }
    const curve = new THREE.CatmullRomCurve3(points);
    const body = new THREE.Mesh(new THREE.TubeGeometry(curve, 180, 0.086, 24, false), jade);
    body.castShadow = true;
    body.receiveShadow = true;
    group.add(body);

    const bellyCurve = new THREE.CatmullRomCurve3(points.map((point, index) => {
        const next = points[Math.min(index + 1, points.length - 1)];
        const tangent = next.clone().sub(point).normalize();
        const side = new THREE.Vector3(-tangent.y, tangent.x, 0).normalize();
        return point.clone().add(side.multiplyScalar(0.052)).add(new THREE.Vector3(0, -0.02, 0.018));
    }));
    const bellyStrip = new THREE.Mesh(new THREE.TubeGeometry(bellyCurve, 180, 0.026, 10, false), belly);
    bellyStrip.castShadow = true;
    group.add(bellyStrip);

    const head = new THREE.Group();
    head.position.copy(points[points.length - 1]);
    head.rotation.set(THREE.MathUtils.degToRad(-3), THREE.MathUtils.degToRad(-38), THREE.MathUtils.degToRad(4));
    head.add(createBox(0.52, 0.3, 0.32, jade, [0, 0.02, 0]));
    head.add(createBox(0.32, 0.14, 0.4, jade, [0.22, -0.025, 0]));
    head.add(createCylinder(0.02, 0.007, 0.42, gold, [-0.1, 0.25, -0.09]));
    head.add(createCylinder(0.02, 0.007, 0.42, gold, [-0.1, 0.25, 0.09]));
    const eyeLeft = createBox(0.03, 0.03, 0.03, gold, [0.2, 0.035, -0.065]);
    const eyeRight = createBox(0.03, 0.03, 0.03, gold, [0.2, 0.035, 0.065]);
    head.add(eyeLeft, eyeRight);
    group.add(head);

    const chest = new THREE.Mesh(new THREE.SphereGeometry(0.18, 24, 16), jade);
    chest.position.copy(points[Math.floor(points.length * 0.84)]);
    chest.scale.set(1.28, 0.95, 1.08);
    chest.castShadow = true;
    chest.receiveShadow = true;
    group.add(chest);

    const whiskerMat = new THREE.MeshStandardMaterial({ color: 0xf1dfae, roughness: 0.52, metalness: 0.08 });
    [-1, 1].forEach(side => {
        const headOrigin = points[points.length - 1];
        const whiskerCurve = new THREE.CatmullRomCurve3([
            headOrigin.clone().add(new THREE.Vector3(0.14, -0.02, side * 0.08)),
            headOrigin.clone().add(new THREE.Vector3(0.5, 0.07, side * 0.27)),
            headOrigin.clone().add(new THREE.Vector3(0.9, -0.1, side * 0.54))
        ]);
        const whisker = new THREE.Mesh(new THREE.TubeGeometry(whiskerCurve, 24, 0.008, 6, false), whiskerMat);
        whisker.castShadow = true;
        group.add(whisker);
    });

    for (let i = 8; i < points.length - 6; i += 6) {
        const spine = createCylinder(0.018, 0.004, 0.13, gold, [points[i].x, points[i].y + 0.055, points[i].z]);
        spine.rotation.z = THREE.MathUtils.degToRad(18);
        group.add(spine);
    }

    [0.18, 0.4, 0.64, 0.86].forEach(t => {
        const index = Math.floor(t * (points.length - 1));
        const point = points[index];
        const claw = createCylinder(0.018, 0.006, 0.18, clawMat, [point.x + 0.09, point.y - 0.04, point.z + 0.1]);
        claw.rotation.z = THREE.MathUtils.degToRad(58);
        group.add(claw);
    });

    return group;
}

function createKatanaFallback() {
    const group = new THREE.Group();
    const blade = new THREE.MeshStandardMaterial({ color: 0xd9d9d6, roughness: 0.2, metalness: 0.86 });
    const edge = new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.18, metalness: 0.72 });
    const handle = new THREE.MeshStandardMaterial({ color: 0x171412, roughness: 0.62 });
    const wrap = new THREE.MeshStandardMaterial({ color: 0x7c1f1f, roughness: 0.68 });
    const wood = new THREE.MeshStandardMaterial({ color: 0x3a281f, roughness: 0.72 });
    const lacquer = new THREE.MeshStandardMaterial({ color: 0x17100d, roughness: 0.5, metalness: 0.08 });
    const gold = new THREE.MeshStandardMaterial({ color: 0xb08a3a, roughness: 0.42, metalness: 0.32 });
    const cord = new THREE.MeshStandardMaterial({ color: 0x9b2722, roughness: 0.76 });

    const plaque = createBox(2.65, 0.55, 0.035, wood, [0.08, -0.08, -0.065]);
    plaque.castShadow = true;
    plaque.receiveShadow = true;
    group.add(plaque);

    group.add(createBox(1.66, 0.042, 0.022, blade, [0.5, 0.08, 0]));
    group.add(createBox(1.55, 0.011, 0.03, edge, [0.55, 0.103, 0]));
    group.add(createBox(0.12, 0.03, 0.028, blade, [1.34, 0.095, 0]));
    group.add(createBox(0.16, 0.12, 0.064, gold, [-0.36, 0.08, 0]));
    group.add(createCylinder(0.034, 0.034, 0.72, handle, [-0.8, 0.08, 0]));
    group.children[group.children.length - 1].rotation.z = Math.PI / 2;

    [-1, -0.86, -0.72, -0.58].forEach(x => {
        const band = createBox(0.04, 0.072, 0.064, wrap, [x, 0, 0]);
        band.position.y += 0.08;
        group.add(band);
    });

    const scabbard = createCylinder(0.032, 0.032, 1.82, lacquer, [0.24, -0.16, 0.012]);
    scabbard.rotation.z = Math.PI / 2;
    group.add(scabbard);
    [-0.67, 1.08].forEach(x => group.add(createBox(0.055, 0.09, 0.062, gold, [x, -0.16, 0.012])));

    [-0.45, 0.15, 0.75].forEach((x, index) => {
        const tie = createBox(0.035, 0.18, 0.022, cord, [x, -0.08, 0.022]);
        tie.rotation.z = THREE.MathUtils.degToRad(index % 2 ? -18 : 18);
        group.add(tie);
    });

    const rackBack = createBox(2.5, 0.12, 0.045, wood, [0.16, -0.31, -0.02]);
    const rackLeft = createBox(0.09, 0.42, 0.095, wood, [-0.9, -0.05, -0.025]);
    const rackRight = createBox(0.09, 0.42, 0.095, wood, [1.12, -0.05, -0.025]);
    const topPegLeft = createCylinder(0.03, 0.03, 0.16, gold, [-0.9, 0.08, 0.03]);
    const topPegRight = createCylinder(0.03, 0.03, 0.16, gold, [1.12, 0.08, 0.03]);
    topPegLeft.rotation.x = Math.PI / 2;
    topPegRight.rotation.x = Math.PI / 2;
    const lowerPegLeft = createCylinder(0.03, 0.03, 0.16, gold, [-0.9, -0.16, 0.03]);
    const lowerPegRight = createCylinder(0.03, 0.03, 0.16, gold, [1.12, -0.16, 0.03]);
    lowerPegLeft.rotation.x = Math.PI / 2;
    lowerPegRight.rotation.x = Math.PI / 2;
    group.add(rackBack, rackLeft, rackRight);
    group.add(topPegLeft, topPegRight, lowerPegLeft, lowerPegRight);

    return group;
}

function renderCategoryFilters() {
    if (!dom.categoryFilters) return;
    const categories = ["all", ...new Set(demoProducts.map(item => item.category))];
    dom.categoryFilters.innerHTML = categories.map(category => `
        <button type="button" class="category-chip${category === "all" ? " is-active" : ""}" data-category="${escapeHtml(category)}">
            ${category === "all" ? "Tất cả" : escapeHtml(category)}
        </button>
    `).join("");
}

function renderProductList() {
    if (!dom.productList) return;
    const products = getFilteredProducts();
    dom.productList.innerHTML = "";
    dom.libraryCount.textContent = `${products.length}/${demoProducts.length} models`;

    products.forEach(productItem => {
        const card = document.createElement("article");
        card.className = "room-product-card";
        card.dataset.category = productItem.category;
        card.innerHTML = `
            <div class="product-thumb">
                ${productItem.thumbnail ? `<img src="${productItem.thumbnail}" alt="${escapeHtml(productItem.name)}">` : categoryIcon(productItem.category)}
            </div>
            <div class="product-meta">
                <div class="product-line">
                    <small>${escapeHtml(productItem.category)}</small>
                    <span>${escapeHtml(productItem.triangles || "3k")}</span>
                </div>
                <h3>${escapeHtml(productItem.name)}</h3>
                <div class="product-card-footer">
                    <span class="product-price">${currencyFormatter.format(productItem.price || 0)}</span>
                    <div class="product-actions">
                        <button type="button" class="room3d-btn room3d-btn-light view-product-btn">Xem</button>
                        <button type="button" class="room3d-btn room3d-btn-primary add-product-btn">Add</button>
                    </div>
                </div>
            </div>
        `;

        const image = card.querySelector("img");
        image?.addEventListener("error", () => {
            image.replaceWith(iconElement(productItem.category));
        });

        card.querySelector(".add-product-btn")?.addEventListener("click", () => addProductToRoom(productItem, card));
        card.querySelector(".view-product-btn")?.addEventListener("click", () => openProductDetail(productItem));
        dom.productList.appendChild(card);
    });
}

function getFilteredProducts() {
    const query = state.searchQuery.trim().toLowerCase();
    return demoProducts.filter(item => {
        const matchesCategory = state.activeCategory === "all" || item.category === state.activeCategory;
        const matchesSearch = !query || `${item.name} ${item.category}`.toLowerCase().includes(query);
        return matchesCategory && matchesSearch;
    });
}

async function addProductToRoom(productItem, card) {
    const button = card?.querySelector(".add-product-btn");
    button?.setAttribute("disabled", "disabled");
    if (button) button.textContent = "Loading";

    try {
        let object = null;
        if (productItem.model3DUrl) {
            try {
                object = await loadModel(productItem);
            } catch (error) {
                console.warn(`Room3D model fallback for ${productItem.id}:`, error);
            }
        }

        if (!object) {
            object = createPlaceholderProduct(productItem);
        }

        placeNewObject(object, productItem);
        setInspectorOpen(true);
        showToast(`${productItem.name} đã được đặt vào phòng.`);
    } finally {
        button?.removeAttribute("disabled");
        if (button) button.textContent = "Add";
    }
}

function loadModel(productItem) {
    const candidates = productItem.model3DUrlCandidates?.length
        ? productItem.model3DUrlCandidates
        : [productItem.model3DUrl].filter(Boolean);

    if (!candidates.length) {
        return Promise.reject(new Error("No model URL configured."));
    }

    return loadModelCandidate(productItem, candidates, 0);
}

function loadModelCandidate(productItem, candidates, index) {
    return new Promise((resolve, reject) => {
        const modelUrl = candidates[index];
        const cached = state.modelCache.get(modelUrl);
        if (cached) {
            const cloned = cloneModelGroup(cached);
            prepareObjectMeshes(cloned);
            applyProductTextureMaps(cloned, productItem);
            normalizeModel(cloned, productItem);
            applyRoomItemMeta(cloned, productItem, false);
            resolve(cloned);
            return;
        }

        const extension = modelUrl.split("?")[0].split(".").pop()?.toLowerCase();
        const loader = extension === "fbx" ? state.fbxLoader : state.gltfLoader;

        loader.load(
            modelUrl,
            asset => {
                const source = new THREE.Group();
                source.add(extension === "fbx" ? asset : asset.scene);
                prepareObjectMeshes(source);
                applyProductTextureMaps(source, productItem);
                state.modelCache.set(modelUrl, source);

                const group = cloneModelGroup(source);
                prepareObjectMeshes(group);
                applyProductTextureMaps(group, productItem);
                normalizeModel(group, productItem);
                applyRoomItemMeta(group, productItem, false);
                resolve(group);
            },
            undefined,
            error => {
                const nextIndex = index + 1;
                if (nextIndex < candidates.length) {
                    loadModelCandidate(productItem, candidates, nextIndex).then(resolve).catch(reject);
                    return;
                }
                reject(error);
            }
        );
    });
}

function applyProductTextureMaps(group, productItem) {
    const textureInfo = TEXTURE_SETS[productItem.model3DUrl];
    if (!textureInfo) return;

    const maps = loadTextureSet(textureInfo);
    group.traverse(child => {
        if (!child.isMesh) return;

        const sourceMaterial = Array.isArray(child.material) ? child.material[0] : child.material;
        const material = new THREE.MeshStandardMaterial({
            color: sourceMaterial?.color?.clone?.() || new THREE.Color(0xffffff),
            map: maps.baseColor,
            normalMap: maps.normal,
            roughnessMap: maps.roughness,
            metalnessMap: maps.metallic,
            roughness: 0.72,
            metalness: 0.04
        });

        if (maps.metallic) material.metalness = 0.18;
        child.material = material;
        child.material.needsUpdate = true;
    });
}

function loadTextureSet(textureInfo) {
    if (textureInfo.cache) return textureInfo.cache;

    const load = url => {
        if (!url) return null;
        const texture = state.textureLoader.load(url);
        texture.colorSpace = url.includes("basecolor") ? THREE.SRGBColorSpace : THREE.NoColorSpace;
        texture.anisotropy = Math.min(8, state.renderer.capabilities.getMaxAnisotropy());
        texture.wrapS = THREE.RepeatWrapping;
        texture.wrapT = THREE.RepeatWrapping;
        return texture;
    };

    textureInfo.cache = {
        baseColor: load(textureInfo.baseColor),
        normal: load(textureInfo.normal),
        roughness: load(textureInfo.roughness),
        metallic: load(textureInfo.metallic)
    };

    return textureInfo.cache;
}

function cloneModelGroup(source) {
    const clone = source.clone(true);
    clone.traverse(child => {
        if (!child.isMesh || !child.material) return;
        child.material = Array.isArray(child.material)
            ? child.material.map(material => material.clone())
            : child.material.clone();
    });
    return clone;
}

function normalizeModel(group, productItem) {
    group.position.set(0, 0, 0);
    group.rotation.set(0, 0, 0);
    group.scale.set(1, 1, 1);
    group.updateMatrixWorld(true);

    const box = new THREE.Box3().setFromObject(group);
    if (box.isEmpty()) return;

    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());
    const maxSize = Math.max(size.x, size.y, size.z) || 1;
    const fitScale = maxSize > 2.45 ? 2.45 / maxSize : 1;
    const content = group.children[0] || group;

    content.position.x -= center.x;
    content.position.y -= box.min.y;
    content.position.z -= center.z;
    group.rotation.y = THREE.MathUtils.degToRad(productItem.rotationY || 0);
    group.scale.setScalar((productItem.scale || 1) * fitScale);
}

function placeNewObject(object, productItem) {
    const spawnPositions = [
        [0, 0, 0.75],
        [1.3, 0, 0.25],
        [-1.2, 0, 0.35],
        [0.7, 0, -1.1],
        [-0.7, 0, -1.15],
        [1.9, 0, 1.25],
        [-1.9, 0, 1.25]
    ];
    const spawn = spawnPositions[state.roomItems.length % spawnPositions.length];
    object.position.set(spawn[0], productItem.offsetY || spawn[1], spawn[2]);
    object.userData.instanceId = `${productItem.id}-${Date.now()}-${Math.round(Math.random() * 1000)}`;
    object.userData.baseScale = object.scale.x || 1;
    object.userData.spawnStart = performance.now();
    object.userData.targetRotationX = object.rotation.x;
    object.userData.targetRotationY = object.rotation.y;
    object.userData.targetRotationZ = object.rotation.z;
    object.scale.multiplyScalar(0.82);

    state.scene.add(object);
    state.roomItems.push({
        instanceId: object.userData.instanceId,
        productId: productItem.id,
        name: productItem.name,
        category: productItem.category,
        price: productItem.price || 0,
        object3D: object,
        quantity: 1
    });

    selectObject(object);
    updateTotal();
}

function createPlaceholderProduct(productItem) {
    let group;
    switch ((productItem.category || "").toLowerCase()) {
        case "sofa":
            group = createSofaPlaceholder();
            break;
        case "table":
            group = createTablePlaceholder();
            break;
        case "chair":
            group = createChairPlaceholder();
            break;
        case "lamp":
            group = createLampPlaceholder(productItem);
            break;
        case "plant":
            group = createPlantPlaceholder(productItem);
            break;
        case "rug":
            group = createRugPlaceholder();
            break;
        case "cabinet":
            group = createCabinetPlaceholder();
            break;
        case "shelf":
            group = createShelfPlaceholder();
            break;
        case "dragon":
            group = createDragonProductPlaceholder(productItem);
            break;
        case "decor":
            group = createDecorPlaceholder();
            break;
        default:
            group = createDefaultPlaceholder();
            break;
    }

    applyRoomItemMeta(group, productItem, true);
    prepareObjectMeshes(group);
    group.rotation.y = THREE.MathUtils.degToRad(productItem.rotationY || 0);
    group.scale.setScalar(productItem.scale || 1);
    return group;
}

function createDragonProductPlaceholder() {
    const group = createShenronFallback();
    normalizeStaticDecorModel(group, {
        maxHeight: 1.35,
        maxWidth: 1.45
    });
    return group;
}

function createSofaPlaceholder() {
    const group = new THREE.Group();
    const fabric = new THREE.MeshStandardMaterial({ color: 0xc9b6a2, roughness: 0.9 });
    const seam = new THREE.MeshStandardMaterial({ color: 0xbca792, roughness: 0.92 });
    group.add(createBox(2.45, 0.44, 0.92, fabric, [0, 0.42, 0]));
    group.add(createBox(2.55, 0.72, 0.22, fabric, [0, 0.78, -0.42]));
    group.add(createBox(0.22, 0.62, 0.95, fabric, [-1.36, 0.58, 0]));
    group.add(createBox(0.22, 0.62, 0.95, fabric, [1.36, 0.58, 0]));
    group.add(createBox(0.7, 0.08, 0.38, seam, [-0.5, 0.68, 0.14]));
    group.add(createBox(0.7, 0.08, 0.38, seam, [0.5, 0.68, 0.14]));
    return group;
}

function createTablePlaceholder() {
    const group = new THREE.Group();
    const wood = new THREE.MeshStandardMaterial({ color: 0x8b5f3d, roughness: 0.78 });
    const dark = new THREE.MeshStandardMaterial({ color: 0x2a211a, roughness: 0.72 });
    group.add(createBox(1.55, 0.14, 0.78, wood, [0, 0.52, 0]));
    [[-0.62, -0.27], [0.62, -0.27], [-0.62, 0.27], [0.62, 0.27]].forEach(([x, z]) => {
        group.add(createBox(0.08, 0.5, 0.08, dark, [x, 0.25, z]));
    });
    return group;
}

function createChairPlaceholder() {
    const group = new THREE.Group();
    const fabric = new THREE.MeshStandardMaterial({ color: 0x9fab94, roughness: 0.86 });
    const wood = new THREE.MeshStandardMaterial({ color: 0x5e412d, roughness: 0.72 });
    group.add(createBox(0.82, 0.18, 0.78, fabric, [0, 0.55, 0]));
    group.add(createBox(0.86, 0.9, 0.16, fabric, [0, 0.98, -0.36]));
    [[-0.32, -0.26], [0.32, -0.26], [-0.32, 0.26], [0.32, 0.26]].forEach(([x, z]) => {
        group.add(createBox(0.08, 0.55, 0.08, wood, [x, 0.27, z]));
    });
    return group;
}

function createLampPlaceholder(productItem, fixed = false) {
    const group = new THREE.Group();
    const metal = new THREE.MeshStandardMaterial({ color: 0x3a3027, roughness: 0.48, metalness: 0.28 });
    const shade = new THREE.MeshStandardMaterial({ color: 0xf2c98b, roughness: 0.7, emissive: 0x503318, emissiveIntensity: 0.18 });
    group.add(createCylinder(0.28, 0.28, 0.06, metal, [0, 0.03, 0]));
    group.add(createCylinder(0.035, 0.035, 1.55, metal, [0, 0.82, 0]));
    group.add(createCylinder(0.28, 0.42, 0.48, shade, [0, 1.72, 0]));
    const glow = new THREE.PointLight(0xffbb76, fixed ? 1.1 : 0.75, 3.2, 2);
    glow.position.set(0, 1.55, 0);
    group.add(glow);
    if (productItem) applyRoomItemMeta(group, productItem, !fixed);
    return group;
}

function createPlantPlaceholder(productItem, fixed = false) {
    const group = new THREE.Group();
    const potMat = new THREE.MeshStandardMaterial({ color: 0x8d5639, roughness: 0.86 });
    const stemMat = new THREE.MeshStandardMaterial({ color: 0x4b321f, roughness: 0.8 });
    const leafMat = new THREE.MeshStandardMaterial({ color: 0x5f8d57, roughness: 0.82 });
    group.add(createCylinder(0.28, 0.22, 0.38, potMat, [0, 0.19, 0]));
    group.add(createCylinder(0.035, 0.035, 0.75, stemMat, [0, 0.72, 0]));
    const leaves = [
        [0.18, 1.1, 0.02, 0.36, 0.16, 0.24],
        [-0.22, 0.98, 0.08, 0.32, 0.14, 0.22],
        [0.02, 1.2, -0.18, 0.3, 0.14, 0.2],
        [0.28, 0.9, -0.12, 0.28, 0.12, 0.2],
        [-0.2, 1.18, -0.08, 0.34, 0.14, 0.22]
    ];
    leaves.forEach(([x, y, z, sx, sy, sz]) => {
        const leaf = new THREE.Mesh(new THREE.SphereGeometry(0.5, 18, 12), leafMat);
        leaf.position.set(x, y, z);
        leaf.scale.set(sx, sy, sz);
        leaf.castShadow = true;
        group.add(leaf);
    });
    if (productItem) applyRoomItemMeta(group, productItem, !fixed);
    return group;
}

function createRugPlaceholder() {
    const group = new THREE.Group();
    const rugMat = new THREE.MeshStandardMaterial({ color: 0xd39a69, roughness: 0.96 });
    group.add(createBox(2.35, 0.03, 1.55, rugMat, [0, 0.025, 0]));
    return group;
}

function createCabinetPlaceholder() {
    const group = new THREE.Group();
    const wood = new THREE.MeshStandardMaterial({ color: 0x806044, roughness: 0.82 });
    const dark = new THREE.MeshStandardMaterial({ color: 0x2b231d, roughness: 0.75 });
    group.add(createBox(1.85, 0.62, 0.42, wood, [0, 0.42, 0]));
    group.add(createBox(0.03, 0.42, 0.43, dark, [0, 0.45, 0]));
    group.add(createBox(1.6, 0.035, 0.46, dark, [0, 0.76, 0]));
    return group;
}

function createShelfPlaceholder() {
    const group = new THREE.Group();
    const wood = new THREE.MeshStandardMaterial({ color: 0x8b684c, roughness: 0.82 });
    const dark = new THREE.MeshStandardMaterial({ color: 0x251d17, roughness: 0.7 });
    group.add(createBox(1.2, 0.08, 0.34, wood, [0, 0.32, 0]));
    group.add(createBox(1.2, 0.08, 0.34, wood, [0, 0.82, 0]));
    group.add(createBox(1.2, 0.08, 0.34, wood, [0, 1.32, 0]));
    group.add(createBox(0.08, 1.2, 0.34, dark, [-0.62, 0.82, 0]));
    group.add(createBox(0.08, 1.2, 0.34, dark, [0.62, 0.82, 0]));
    return group;
}

function createDecorPlaceholder() {
    const group = new THREE.Group();
    const clay = new THREE.MeshStandardMaterial({ color: 0xd7c8b6, roughness: 0.7 });
    const accent = new THREE.MeshStandardMaterial({ color: 0x2c2520, roughness: 0.58, metalness: 0.08 });
    group.add(createCylinder(0.22, 0.16, 0.48, clay, [-0.18, 0.24, 0]));
    group.add(createCylinder(0.13, 0.2, 0.36, clay, [0.2, 0.18, 0.08]));
    group.add(createBox(0.56, 0.04, 0.28, accent, [0, 0.03, 0]));
    return group;
}

function createDefaultPlaceholder() {
    const group = new THREE.Group();
    const mat = new THREE.MeshStandardMaterial({ color: 0xb9a48d, roughness: 0.86 });
    group.add(createBox(1, 0.8, 1, mat, [0, 0.4, 0]));
    return group;
}

function applyRoomItemMeta(group, productItem, isPlaceholder) {
    group.userData = {
        ...group.userData,
        type: "room-item",
        productId: productItem.id,
        name: productItem.name,
        category: productItem.category,
        price: productItem.price || 0,
        qualityTier: productItem.qualityTier,
        triangles: productItem.triangles,
        isPlaceholder,
        offsetY: productItem.offsetY || 0,
        defaultRotationY: productItem.rotationY || 0
    };
}

function bindEvents() {
    dom.canvas.addEventListener("pointerdown", handlePointerDown);
    dom.canvas.addEventListener("pointermove", handlePointerMove);
    window.addEventListener("pointerup", handlePointerUp);
    window.addEventListener("resize", () => {
        handleResize();
        handleDetailResize();
    });

    dom.rotateLeftBtn?.addEventListener("click", () => rotateSelected(-15));
    dom.rotateRightBtn?.addEventListener("click", () => rotateSelected(15));
    dom.pitchUpBtn?.addEventListener("click", () => rotateSelectedAxis("x", -10));
    dom.pitchDownBtn?.addEventListener("click", () => rotateSelectedAxis("x", 10));
    dom.rollLeftBtn?.addEventListener("click", () => rotateSelectedAxis("z", -10));
    dom.rollRightBtn?.addEventListener("click", () => rotateSelectedAxis("z", 10));
    dom.focusSelectedBtn?.addEventListener("click", focusSelected);
    dom.resetRotationBtn?.addEventListener("click", resetSelectedRotation);
    dom.deleteBtn?.addEventListener("click", deleteSelected);
    dom.snapshotBtn?.addEventListener("click", takeSnapshot);
    dom.buyRoomBtn?.addEventListener("click", buyRoom);
    dom.resetCameraBtn?.addEventListener("click", resetCamera);
    dom.fullscreenBtn?.addEventListener("click", toggleFullscreen);
    dom.detailCloseBtn?.addEventListener("click", closeProductDetail);
    dom.detailAutoRotateBtn?.addEventListener("click", toggleDetailAutoRotate);
    dom.detailAddBtn?.addEventListener("click", () => {
        if (!state.detailProduct) return;
        const productItem = state.detailProduct;
        closeProductDetail();
        addProductToRoom(productItem, null);
    });
    dom.libraryToggleBtn?.addEventListener("click", () => setLibraryOpen(dom.page.dataset.libraryOpen !== "true"));
    dom.libraryCloseBtn?.addEventListener("click", () => setLibraryOpen(false));
    dom.inspectorToggleBtn?.addEventListener("click", () => setInspectorOpen(dom.page.dataset.inspectorOpen !== "true"));
    dom.inspectorCloseBtn?.addEventListener("click", () => setInspectorOpen(false));
    dom.productSearchInput?.addEventListener("input", event => {
        state.searchQuery = event.target.value;
        renderProductList();
    });
    dom.categoryFilters?.addEventListener("click", event => {
        const button = event.target.closest("[data-category]");
        if (!button) return;
        state.activeCategory = button.dataset.category;
        dom.categoryFilters.querySelectorAll(".category-chip").forEach(chip => {
            chip.classList.toggle("is-active", chip === button);
        });
        renderProductList();
    });
    window.addEventListener("keydown", event => {
        if (event.key === "Escape" && dom.page.dataset.detailOpen === "true") {
            closeProductDetail();
        }
    });
    document.addEventListener("fullscreenchange", updateFullscreenButton);
}

function setLibraryOpen(open) {
    dom.page.dataset.libraryOpen = open ? "true" : "false";
    dom.libraryToggleBtn?.setAttribute("aria-expanded", String(open));
}

function setInspectorOpen(open) {
    dom.page.dataset.inspectorOpen = open ? "true" : "false";
    dom.inspectorToggleBtn?.setAttribute("aria-expanded", String(open));
}

function handlePointerDown(event) {
    setPointerFromEvent(event);
    const hitObject = getRoomItemFromPointer();

    if (!hitObject) {
        selectObject(null);
        return;
    }

    selectObject(hitObject);
    setInspectorOpen(true);
    state.activePointerId = event.pointerId;
    dom.canvas.setPointerCapture?.(event.pointerId);

    state.raycaster.setFromCamera(state.pointer, state.camera);
    if (state.raycaster.ray.intersectPlane(state.dragPlane, state.dragPoint)) {
        state.dragOffset.copy(hitObject.position).sub(state.dragPoint);
        state.isDraggingObject = true;
        state.orbitControls.enabled = false;
    }
}

function handlePointerMove(event) {
    if (!state.isDraggingObject || !state.selectedObject) return;

    setPointerFromEvent(event);
    state.raycaster.setFromCamera(state.pointer, state.camera);
    if (!state.raycaster.ray.intersectPlane(state.dragPlane, state.dragPoint)) return;

    const target = state.dragPoint.add(state.dragOffset);
    state.selectedObject.position.x = THREE.MathUtils.clamp(target.x, -3.45, 3.45);
    state.selectedObject.position.z = THREE.MathUtils.clamp(target.z, -3.45, 3.45);
    state.selectedObject.position.y = state.selectedObject.userData.offsetY || 0;
    updateSelectionBox();
}

function handlePointerUp() {
    if (state.activePointerId !== null) {
        dom.canvas.releasePointerCapture?.(state.activePointerId);
    }

    state.activePointerId = null;
    state.isDraggingObject = false;
    if (state.orbitControls) state.orbitControls.enabled = true;
}

function getRoomItemFromPointer() {
    state.raycaster.setFromCamera(state.pointer, state.camera);
    const roots = state.roomItems.map(item => item.object3D);
    const intersections = state.raycaster.intersectObjects(roots, true);
    if (!intersections.length) return null;
    return findRoomItemRoot(intersections[0].object);
}

function findRoomItemRoot(object) {
    let current = object;
    while (current) {
        if (current.userData?.type === "room-item") return current;
        current = current.parent;
    }
    return null;
}

function selectObject(object) {
    state.selectedObject = object;

    if (state.selectionBox) {
        state.scene.remove(state.selectionBox);
        state.selectionBox.geometry?.dispose();
        state.selectionBox.material?.dispose?.();
        state.selectionBox = null;
    }

    if (state.selectionHalo) {
        state.scene.remove(state.selectionHalo);
        state.selectionHalo.geometry?.dispose();
        state.selectionHalo.material?.dispose?.();
        state.selectionHalo = null;
    }

    if (object) {
        state.selectionBox = new THREE.BoxHelper(object, 0xff9c3f);
        state.selectionBox.material.depthTest = false;
        state.selectionBox.renderOrder = 999;
        state.scene.add(state.selectionBox);

        state.selectionHalo = createSelectionHalo(object);
        state.scene.add(state.selectionHalo);
    }

    updateInspector();
}

function rotateSelected(degrees) {
    return rotateSelectedAxis("y", degrees);
}

function rotateSelectedAxis(axis, degrees) {
    if (!state.selectedObject) {
        showToast("Hãy chọn một sản phẩm trong phòng trước.");
        return;
    }

    const normalizedAxis = axis.toLowerCase();
    if (!["x", "y", "z"].includes(normalizedAxis)) return;

    const key = `targetRotation${normalizedAxis.toUpperCase()}`;
    const target = (state.selectedObject.userData[key] ?? state.selectedObject.rotation[normalizedAxis]) + THREE.MathUtils.degToRad(degrees);
    state.selectedObject.userData[key] = target;
}

function resetSelectedRotation() {
    if (!state.selectedObject) {
        showToast("ChÆ°a cÃ³ sáº£n pháº©m nÃ o Ä‘Æ°á»£c chá»n.");
        return;
    }

    state.selectedObject.userData.targetRotationX = 0;
    state.selectedObject.userData.targetRotationY = THREE.MathUtils.degToRad(state.selectedObject.userData.defaultRotationY || 0);
    state.selectedObject.userData.targetRotationZ = 0;
}

function deleteSelected() {
    if (!state.selectedObject) {
        showToast("Chưa có sản phẩm nào được chọn.");
        return;
    }

    const object = state.selectedObject;
    state.scene.remove(object);
    disposeObject(object);
    state.roomItems = state.roomItems.filter(item => item.object3D !== object);
    selectObject(null);
    updateTotal();
}

function updateInspector() {
    const object = state.selectedObject;
    const hasSelection = Boolean(object);
    dom.page.dataset.hasSelection = hasSelection ? "true" : "false";
    [
        dom.rotateLeftBtn,
        dom.rotateRightBtn,
        dom.pitchUpBtn,
        dom.pitchDownBtn,
        dom.rollLeftBtn,
        dom.rollRightBtn,
        dom.focusSelectedBtn,
        dom.resetRotationBtn,
        dom.deleteBtn
    ].forEach(btn => {
        if (!btn) return;
        btn.disabled = !hasSelection;
    });

    if (!object) {
        dom.selectedInfo.className = "selected-info empty";
        dom.selectedInfo.textContent = "Chưa chọn sản phẩm";
        return;
    }

    dom.selectedInfo.className = "selected-info";
    dom.selectedInfo.innerHTML = `
        <strong>${escapeHtml(object.userData.name || "Furniture item")}</strong>
        <span>${escapeHtml(object.userData.category || "furniture")} · ${object.userData.isPlaceholder ? "styled fallback" : "real 3D model"} · ${escapeHtml(object.userData.triangles || "3k")}</span>
        <span class="selected-price">${currencyFormatter.format(object.userData.price || 0)}</span>
        <em>Đang chọn: kéo trên sàn để di chuyển, dùng nút bên dưới để xoay/focus/xóa.</em>
    `;
}

function updateTotal() {
    const total = state.roomItems.reduce((sum, item) => sum + (item.price || 0) * (item.quantity || 1), 0);
    dom.totalPrice.textContent = currencyFormatter.format(total);
}

function takeSnapshot() {
    if (!state.renderer) return;

    state.renderer.render(state.scene, state.camera);
    const dataUrl = state.renderer.domElement.toDataURL("image/png");
    const link = document.createElement("a");
    link.href = dataUrl;
    link.download = `furnish-room-${Date.now()}.png`;
    link.click();
    showToast("Đã chụp ảnh showroom 3D.");
}

function buyRoom() {
    if (!state.roomItems.length) {
        showToast("Hãy thêm ít nhất một sản phẩm vào phòng.");
        return;
    }

    const total = state.roomItems.reduce((sum, item) => sum + item.price, 0);
    showToast(`Đã chọn ${state.roomItems.length} sản phẩm, tổng ${currencyFormatter.format(total)}. Checkout bộ phòng sẽ được kết nối sau.`);
    // TODO: Later POST roomItems to /Cart/AddRoomItems when the backend endpoint exists.
}

async function toggleFullscreen() {
    if (!document.fullscreenElement) {
        try {
            await dom.page.requestFullscreen();
        } catch {
            showToast("Trình duyệt không cho mở full màn hình lúc này.");
        }
    } else {
        await document.exitFullscreen();
    }
}

function updateFullscreenButton() {
    const isFullscreen = document.fullscreenElement === dom.page;
    dom.page.dataset.fullscreen = isFullscreen ? "true" : "false";
    if (!dom.fullscreenBtn) return;

    dom.fullscreenBtn.innerHTML = isFullscreen
        ? `<i class="bi bi-fullscreen-exit"></i><span>Thoát full</span>`
        : `<i class="bi bi-fullscreen"></i><span>Full màn hình</span>`;
    handleResize();
    handleDetailResize();
}

async function openProductDetail(productItem) {
    state.detailProduct = productItem;
    dom.page.dataset.detailOpen = "true";
    dom.detailLoading?.classList.remove("is-hidden");
    dom.detailProductName.textContent = productItem.name;
    dom.detailProductMeta.textContent = `${productItem.category} · ${productItem.triangles || "3k"} · ${currencyFormatter.format(productItem.price || 0)}`;

    initDetailViewer();
    clearDetailObject();
    handleDetailResize();

    try {
        let object = null;
        if (productItem.model3DUrl) {
            try {
                object = await loadModel(productItem);
            } catch (error) {
                console.warn(`Room3D detail fallback for ${productItem.id}:`, error);
            }
        }

        if (!object) object = createPlaceholderProduct(productItem);
        prepareDetailObject(object);
        state.detailViewer.scene.add(object);
        state.detailViewer.object = object;
        fitDetailCameraToObject(object);
    } finally {
        dom.detailLoading?.classList.add("is-hidden");
    }
}

function closeProductDetail() {
    dom.page.dataset.detailOpen = "false";
    state.detailProduct = null;
}

function toggleDetailAutoRotate() {
    if (!state.detailViewer) return;

    state.detailViewer.autoRotate = !state.detailViewer.autoRotate;
    updateDetailAutoRotateButton();
}

function updateDetailAutoRotateButton() {
    if (!dom.detailAutoRotateBtn || !state.detailViewer) return;

    const enabled = Boolean(state.detailViewer.autoRotate);
    dom.detailAutoRotateBtn.setAttribute("aria-pressed", String(enabled));
    dom.detailAutoRotateBtn.classList.toggle("is-active", enabled);
    dom.detailAutoRotateBtn.innerHTML = enabled
        ? `<i class="bi bi-pause-circle"></i><span>Dừng quay</span>`
        : `<i class="bi bi-arrow-repeat"></i><span>Tự quay</span>`;
}

function initDetailViewer() {
    if (state.detailViewer || !dom.detailCanvas) return;

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0xf4eee6);
    scene.fog = new THREE.Fog(0xf4eee6, 8, 16);

    const camera = new THREE.PerspectiveCamera(42, 1, 0.1, 100);
    camera.position.set(2.8, 1.8, 3.2);

    const renderer = new THREE.WebGLRenderer({
        canvas: dom.detailCanvas,
        antialias: true,
        alpha: false
    });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.08;
    renderer.outputColorSpace = THREE.SRGBColorSpace;

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.06;
    controls.minDistance = 1.2;
    controls.maxDistance = 8;
    controls.maxPolarAngle = Math.PI / 1.95;
    controls.target.set(0, 0.75, 0);

    const hemi = new THREE.HemisphereLight(0xfff4e8, 0x575b62, 2.4);
    const key = new THREE.DirectionalLight(0xffe4ba, 3.2);
    key.position.set(3.5, 4.5, 3.6);
    key.castShadow = true;
    key.shadow.mapSize.set(2048, 2048);
    const rim = new THREE.PointLight(0xff9c3f, 1.1, 6, 2);
    rim.position.set(-2.4, 2.1, -1.8);
    const floor = createBox(5, 0.04, 5, new THREE.MeshStandardMaterial({ color: 0xd4b58d, roughness: 0.82 }), [0, -0.03, 0]);
    floor.receiveShadow = true;
    scene.add(hemi, key, rim, floor);

    state.detailViewer = { scene, camera, renderer, controls, object: null, autoRotate: false };
    updateDetailAutoRotateButton();
}

function clearDetailObject() {
    if (!state.detailViewer?.object) return;
    state.detailViewer.scene.remove(state.detailViewer.object);
    disposeObject(state.detailViewer.object);
    state.detailViewer.object = null;
}

function prepareDetailObject(object) {
    object.position.set(0, 0, 0);
    object.rotation.y = THREE.MathUtils.degToRad(18);
    object.userData.targetRotationY = object.rotation.y;
}

function fitDetailCameraToObject(object) {
    const box = new THREE.Box3().setFromObject(object);
    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());
    const maxSize = Math.max(size.x, size.y, size.z, 1);
    const distance = THREE.MathUtils.clamp(maxSize * 2.1, 2.4, 6.2);

    state.detailViewer.controls.target.set(center.x, Math.max(0.55, center.y), center.z);
    state.detailViewer.camera.position.set(center.x + distance, Math.max(1.25, center.y + maxSize * 0.55), center.z + distance);
    state.detailViewer.camera.lookAt(state.detailViewer.controls.target);
    state.detailViewer.controls.update();
}

function handleDetailResize() {
    if (!state.detailViewer || !dom.detailCanvas) return;

    const rect = dom.detailCanvas.getBoundingClientRect();
    const width = Math.max(1, rect.width);
    const height = Math.max(1, rect.height);
    state.detailViewer.camera.aspect = width / height;
    state.detailViewer.camera.updateProjectionMatrix();
    state.detailViewer.renderer.setSize(width, height, false);
}

function resetCamera() {
    animateCameraTo(new THREE.Vector3(5, 3, 6), new THREE.Vector3(0, 1, 0));
}

function focusSelected() {
    if (!state.selectedObject) {
        showToast("Hãy chọn một sản phẩm trong phòng trước.");
        return;
    }

    const box = new THREE.Box3().setFromObject(state.selectedObject);
    const center = box.getCenter(new THREE.Vector3());
    const position = center.clone().add(new THREE.Vector3(2.5, 1.65, 2.7));
    animateCameraTo(position, new THREE.Vector3(center.x, Math.max(0.7, center.y), center.z));
}

function animateCameraTo(position, target) {
    state.cameraAnimation = {
        startTime: performance.now(),
        duration: 700,
        fromPosition: state.camera.position.clone(),
        toPosition: position,
        fromTarget: state.orbitControls.target.clone(),
        toTarget: target
    };
}

function animate() {
    state.animationFrame = requestAnimationFrame(animate);
    state.orbitControls?.update();
    animateCamera();
    animateSpawnedItems();
    animateRotations();
    updateSelectionBox();
    state.renderer.render(state.scene, state.camera);
    renderDetailViewer();
}

function renderDetailViewer() {
    if (!state.detailViewer || dom.page.dataset.detailOpen !== "true") return;

    if (state.detailViewer.object && state.detailViewer.autoRotate) {
        state.detailViewer.object.rotation.y += 0.0025;
    }

    state.detailViewer.controls.update();
    state.detailViewer.renderer.render(state.detailViewer.scene, state.detailViewer.camera);
}

function animateCamera() {
    if (!state.cameraAnimation) return;

    const elapsed = Math.min((performance.now() - state.cameraAnimation.startTime) / state.cameraAnimation.duration, 1);
    const ease = 1 - Math.pow(1 - elapsed, 3);
    state.camera.position.lerpVectors(state.cameraAnimation.fromPosition, state.cameraAnimation.toPosition, ease);
    state.orbitControls.target.lerpVectors(state.cameraAnimation.fromTarget, state.cameraAnimation.toTarget, ease);

    if (elapsed >= 1) {
        state.cameraAnimation = null;
    }
}

function animateSpawnedItems() {
    const now = performance.now();
    state.roomItems.forEach(item => {
        const object = item.object3D;
        if (!object.userData.spawnStart) return;

        const elapsed = Math.min((now - object.userData.spawnStart) / 360, 1);
        const ease = 1 - Math.pow(1 - elapsed, 3);
        const baseScale = object.userData.baseScale || 1;
        object.scale.setScalar(baseScale * (0.82 + 0.18 * ease));

        if (elapsed >= 1) {
            object.userData.spawnStart = null;
            object.scale.setScalar(baseScale);
        }
    });
}

function animateRotations() {
    state.roomItems.forEach(item => {
        const object = item.object3D;
        ["x", "y", "z"].forEach(axis => {
            const key = `targetRotation${axis.toUpperCase()}`;
            if (object.userData[key] === undefined) return;
            object.rotation[axis] = THREE.MathUtils.lerp(object.rotation[axis], object.userData[key], 0.18);
        });
    });
}

function updateSelectionBox() {
    state.selectionBox?.update();
    if (state.selectionHalo && state.selectedObject) {
        state.selectionHalo.position.x = state.selectedObject.position.x;
        state.selectionHalo.position.z = state.selectedObject.position.z;
        state.selectionHalo.rotation.z += 0.012;
    }
}

function createSelectionHalo(object) {
    const box = new THREE.Box3().setFromObject(object);
    const size = box.getSize(new THREE.Vector3());
    const radius = Math.max(0.55, Math.min(1.85, Math.max(size.x, size.z) * 0.62));
    const halo = new THREE.Mesh(
        new THREE.RingGeometry(radius * 0.82, radius, 72),
        new THREE.MeshBasicMaterial({
            color: 0xff9c3f,
            transparent: true,
            opacity: 0.42,
            depthWrite: false,
            side: THREE.DoubleSide
        })
    );
    halo.rotation.x = -Math.PI / 2;
    halo.position.set(object.position.x, 0.028, object.position.z);
    halo.renderOrder = 998;
    return halo;
}

function handleResize() {
    if (!state.renderer || !state.camera) return;

    const rect = dom.canvas.getBoundingClientRect();
    const width = Math.max(1, rect.width);
    const height = Math.max(1, rect.height);
    state.camera.aspect = width / height;
    state.camera.updateProjectionMatrix();
    state.renderer.setSize(width, height, false);
}

function setPointerFromEvent(event) {
    const rect = dom.canvas.getBoundingClientRect();
    state.pointer.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    state.pointer.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;
}

function createBox(width, height, depth, material, position = [0, 0, 0]) {
    const mesh = new THREE.Mesh(new THREE.BoxGeometry(width, height, depth), material);
    mesh.position.set(position[0], position[1], position[2]);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    return mesh;
}

function createCylinder(radiusTop, radiusBottom, height, material, position = [0, 0, 0]) {
    const mesh = new THREE.Mesh(new THREE.CylinderGeometry(radiusTop, radiusBottom, height, 32), material);
    mesh.position.set(position[0], position[1], position[2]);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    return mesh;
}

function prepareObjectMeshes(object) {
    object.traverse(child => {
        if (!child.isMesh) return;
        child.castShadow = true;
        child.receiveShadow = true;
        if (child.material) {
            child.material.needsUpdate = true;
        }
    });
}

function disposeObject(object) {
    object.traverse(child => {
        if (!child.isMesh) return;
        child.geometry?.dispose();
        if (Array.isArray(child.material)) {
            child.material.forEach(material => material.dispose?.());
        } else {
            child.material?.dispose?.();
        }
    });
}

function showToast(message) {
    if (!dom.toast) return;

    dom.toast.textContent = message;
    dom.toast.classList.add("is-visible");
    window.clearTimeout(showToast.timeoutId);
    showToast.timeoutId = window.setTimeout(() => {
        dom.toast.classList.remove("is-visible");
    }, 3600);
}

function asset(key, fileName) {
    return {
        model: `/models/demo-products/sharetextures/${key}/${fileName}.fbx`,
        thumb: `/models/demo-products/sharetextures/${key}/${fileName}-preview.webp`
    };
}

function textureSet(key, prefix, resolution = "1K") {
    const root = `/models/demo-products/sharetextures/${key}`;
    return {
        baseColor: `${root}/${prefix}_basecolor-${resolution}.png`,
        normal: `${root}/${prefix}_normal-${resolution}.png`,
        roughness: `${root}/${prefix}_roughness-${resolution}.png`,
        metallic: `${root}/${prefix}_metallic-${resolution}.png`,
        cache: null
    };
}

function product(id, name, category, price, model3DUrl, thumbnailOrScale, scaleOrOffsetY, offsetYOrRotationY, rotationYOrQualityTier, qualityTierOrTriangles, trianglesOrBg, bgOrFg, fgMaybe) {
    const hasCustomThumbnail = typeof thumbnailOrScale === "string" || thumbnailOrScale === null;
    const thumbnail = hasCustomThumbnail ? thumbnailOrScale : null;
    const scale = hasCustomThumbnail ? scaleOrOffsetY : thumbnailOrScale;
    const offsetY = hasCustomThumbnail ? offsetYOrRotationY : scaleOrOffsetY;
    const rotationY = hasCustomThumbnail ? rotationYOrQualityTier : offsetYOrRotationY;
    const qualityTier = hasCustomThumbnail ? qualityTierOrTriangles : rotationYOrQualityTier;
    const triangles = hasCustomThumbnail ? trianglesOrBg : qualityTierOrTriangles;
    const bg = hasCustomThumbnail ? bgOrFg : trianglesOrBg;
    const fg = hasCustomThumbnail ? fgMaybe : bgOrFg;

    return {
        id,
        name,
        category,
        price,
        thumbnail: thumbnail || createThumb(category, bg, fg),
        model3DUrl,
        scale,
        offsetY,
        rotationY,
        qualityTier,
        triangles
    };
}

function withModelCandidates(productItem, candidates) {
    return {
        ...productItem,
        model3DUrlCandidates: candidates
    };
}

function categoryIcon(category) {
    const icon = {
        sofa: "bi-lamp",
        table: "bi-grid-3x3-gap",
        chair: "bi-easel",
        lamp: "bi-lightbulb",
        plant: "bi-flower1",
        rug: "bi-border-style",
        cabinet: "bi-collection",
        decor: "bi-stars",
        dragon: "bi-stars",
        shelf: "bi-bookshelf"
    }[category] || "bi-box";
    return `<i class="bi ${icon}"></i>`;
}

function iconElement(category) {
    const wrapper = document.createElement("span");
    wrapper.innerHTML = categoryIcon(category);
    return wrapper.firstElementChild;
}

function createThumb(label, bg, fg) {
    const svg = `
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160">
            <defs>
                <linearGradient id="g" x1="0" x2="1" y1="0" y2="1">
                    <stop offset="0" stop-color="${bg}"/>
                    <stop offset="1" stop-color="#f8efe4"/>
                </linearGradient>
            </defs>
            <rect width="160" height="160" rx="28" fill="url(#g)"/>
            <circle cx="126" cy="34" r="24" fill="rgba(255,255,255,.32)"/>
            <text x="80" y="82" text-anchor="middle" font-family="Arial, sans-serif" font-size="20" font-weight="800" fill="${fg}">${label}</text>
            <rect x="36" y="108" width="88" height="10" rx="5" fill="${fg}" opacity=".35"/>
        </svg>`;
    return `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}`;
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}
