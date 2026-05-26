import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js";
import { FBXLoader } from "three/addons/loaders/FBXLoader.js";

const currencyFormatter = new Intl.NumberFormat("vi-VN", {
  style: "currency",
  currency: "VND",
});

const MODEL_PATHS = {
  sofa: "/models/demo-products/sofa/sofa_02_1k.gltf",
  table: "/models/demo-products/coffee-table/modern_coffee_table_01_1k.gltf",
  chair: "/models/demo-products/lounge-chair/ArmChair_01_1k.gltf",
  plant: "/models/demo-products/plant/potted_plant_04_1k.gltf",
  cabinet: "/models/demo-products/tv-stand/drawer_cabinet_1k.gltf",
};

const WINDOW_SCARE_MODEL_URL =
  "/models/rooms/scare/tung-sahur-2k/tung_sahur_2k.gltf";
const PENDING_ROOM_CART_KEY = "room3d.pendingCartItems";

const SHARETEXTURES = {
  sofa22: asset("sofa-22", "sofa-22"),
  sofa21: asset("sofa-21", "sofa-21"),
  armchair16: asset("armchair-16", "armchair-16"),
  armchair15: asset("armchair-15", "armchair-15"),
  armchair14: asset("armchair-14", "armchair-14"),
  armchair13: asset("armchair-13", "armchair-13"),
  table5: asset("table-5", "table-5"),
  table4: asset("table-4", "table-4"),
  stool9: asset("stool-9", "stool-9"),
};

const TEXTURE_SETS = {
  [SHARETEXTURES.sofa22.model]: textureSet("sofa-22", "3D_sofa_22", "2K"),
  [SHARETEXTURES.sofa21.model]: textureSet("sofa-21", "3D_sofa_21", "2K"),
  [SHARETEXTURES.armchair16.model]: textureSet(
    "armchair-16",
    "3D_armchair_16",
    "2K",
  ),
  [SHARETEXTURES.armchair15.model]: textureSet("armchair-15", "3D_armchair_14"),
  [SHARETEXTURES.armchair14.model]: textureSet("armchair-14", "3D_armchair_14"),
  [SHARETEXTURES.armchair13.model]: textureSet("armchair-13", "3D_armchair_13"),
  [SHARETEXTURES.table5.model]: textureSet("table-5", "picnic_table_01"),
  [SHARETEXTURES.table4.model]: textureSet("table-4", "Table_4"),
  [SHARETEXTURES.stool9.model]: textureSet("stool-9", "folding_camping_stool"),
};

const STATIC_DECOR = [
  {
    id: "katana-wall",
    name: "Ceremonial Katana Wall Display",
    model3DUrl: "/models/rooms/decor/katana-bright/katana_goldberg_bright.glb",
    model3DUrlCandidates: [
      "/models/rooms/decor/katana-bright/katana_goldberg_bright.glb",
    ],
    fallback: "katana",
    position: [-3.82, 1.72, 0],
    rotation: [0, 90, 0],
    modelPreRotation: [90, 0, 90],
    modelOffset: [0, 0.1, 0],
    maxHeight: 0.95,
    maxWidth: 3.8,
    credit: "Katana Low-Poly by GoldbergR (CC BY 4.0)",
  },
];

const demoProducts = [
  withModelCandidates(
    product(
      "dragon-01",
      "Shenron Guardian Dragon",
      "dragon",
      20000000000,
      "/models/rooms/decor/shenron-yanez.glb",
      null,
      1,
      0,
      18,
      "signature",
      "10k",
      "#58d6a0",
      "#d8c782",
    ),
    ["/models/rooms/decor/shenron-yanez.glb"],
  ),
  product(
    "sofa-01",
    "Modern L-Shaped Sofa",
    "sofa",
    15990000,
    SHARETEXTURES.sofa22.model,
    SHARETEXTURES.sofa22.thumb,
    1.02,
    0,
    0,
    "hero",
    "8k-10k feel",
    "#d8b48c",
    "#6e4a32",
  ),
  product(
    "sofa-02",
    "Velvet Sectional Sofa",
    "sofa",
    22990000,
    SHARETEXTURES.sofa21.model,
    SHARETEXTURES.sofa21.thumb,
    1,
    0,
    -8,
    "hero",
    "8k-10k feel",
    "#e6d4c0",
    "#5c4636",
  ),
  product(
    "sofa-03",
    "Nordic 3-Seater Sofa",
    "sofa",
    8990000,
    MODEL_PATHS.sofa,
    null,
    0.82,
    0,
    10,
    "standard",
    "7k",
    "#c7b7a1",
    "#4d3928",
  ),
  product(
    "table-01",
    "Minimalist Coffee Table",
    "table",
    1990000,
    SHARETEXTURES.table5.model,
    SHARETEXTURES.table5.thumb,
    1,
    0,
    0,
    "standard",
    "3k",
    "#c69c6d",
    "#2d2219",
  ),
  product(
    "table-02",
    "Marble Top Table",
    "table",
    4590000,
    SHARETEXTURES.table4.model,
    SHARETEXTURES.table4.thumb,
    1,
    0,
    20,
    "standard",
    "3k",
    "#b27d4e",
    "#2c2018",
  ),
  product(
    "table-03",
    "Glass Top Side Table",
    "table",
    990000,
    MODEL_PATHS.table,
    null,
    1.05,
    0,
    -12,
    "standard",
    "4k",
    "#7a6a5a",
    "#181513",
  ),
  product(
    "chair-01",
    "Classic Leather Armchair",
    "chair",
    4990000,
    SHARETEXTURES.armchair16.model,
    SHARETEXTURES.armchair16.thumb,
    1,
    0,
    0,
    "hero",
    "8k-10k feel",
    "#bfc7b2",
    "#37402f",
  ),
  product(
    "chair-02",
    "Curved Accent Chair",
    "chair",
    3890000,
    SHARETEXTURES.armchair15.model,
    SHARETEXTURES.armchair15.thumb,
    1,
    0,
    18,
    "standard",
    "3k",
    "#d7cab8",
    "#5a4b3d",
  ),
  product(
    "chair-03",
    "Chaise Lounge Chair",
    "chair",
    5990000,
    SHARETEXTURES.armchair14.model,
    SHARETEXTURES.armchair14.thumb,
    1,
    0,
    -14,
    "standard",
    "3k",
    "#aeb7aa",
    "#303b34",
  ),
  product(
    "chair-04",
    "Ergonomic Office Chair",
    "chair",
    2990000,
    SHARETEXTURES.armchair13.model,
    SHARETEXTURES.armchair13.thumb,
    1,
    0,
    12,
    "standard",
    "3k",
    "#d5b45f",
    "#49381f",
  ),
  product(
    "stool-01",
    "Storage Ottoman",
    "chair",
    1290000,
    SHARETEXTURES.stool9.model,
    SHARETEXTURES.stool9.thumb,
    1,
    0,
    0,
    "standard",
    "2k",
    "#c9aa84",
    "#5c412b",
  ),
  product(
    "lamp-01",
    "Corner Floor Lamp",
    "lamp",
    890000,
    null,
    1,
    0,
    0,
    "fallback",
    "2k",
    "#f2c16d",
    "#4d3823",
  ),
  product(
    "lamp-02",
    "Metal Floor Lamp",
    "lamp",
    690000,
    null,
    1,
    0,
    0,
    "fallback",
    "2k",
    "#e9bb73",
    "#3f3328",
  ),
  product(
    "lamp-03",
    "Desk Lamp LED",
    "lamp",
    590000,
    null,
    0.88,
    0,
    0,
    "fallback",
    "2k",
    "#f0d3a1",
    "#403022",
  ),
  product(
    "plant-01",
    "Artificial Plant Tree",
    "plant",
    1290000,
    MODEL_PATHS.plant,
    1,
    0,
    0,
    "standard",
    "3k",
    "#7ca36f",
    "#254b2b",
  ),
  product(
    "plant-02",
    "Artificial Plant Tree (Tall)",
    "plant",
    1290000,
    MODEL_PATHS.plant,
    1.25,
    0,
    -10,
    "standard",
    "3k",
    "#6f9b66",
    "#243d24",
  ),
  product(
    "plant-03",
    "Artificial Plant Tree (Mini)",
    "plant",
    1290000,
    MODEL_PATHS.plant,
    0.65,
    0,
    14,
    "standard",
    "3k",
    "#8bb67d",
    "#2f4c2d",
  ),
  product(
    "rug-01",
    "Candle Holder Set",
    "rug",
    290000,
    null,
    1,
    0,
    0,
    "fallback",
    "1k",
    "#d8a36f",
    "#9d5d30",
  ),
  product(
    "rug-02",
    "Candle Holder Set",
    "rug",
    290000,
    null,
    1.15,
    0,
    0,
    "fallback",
    "1k",
    "#c9b99f",
    "#7d674d",
  ),
  product(
    "rug-03",
    "Candle Holder Set",
    "rug",
    290000,
    null,
    0.95,
    0,
    0,
    "fallback",
    "1k",
    "#c77f55",
    "#6d3e2a",
  ),
  product(
    "cabinet-01",
    "Oak Wood TV Stand",
    "cabinet",
    3490000,
    MODEL_PATHS.cabinet,
    1,
    0,
    0,
    "standard",
    "5k",
    "#9f7b55",
    "#2c2118",
  ),
  product(
    "cabinet-02",
    "Filing Cabinet 4-drawer",
    "cabinet",
    1890000,
    MODEL_PATHS.cabinet,
    1.14,
    0,
    0,
    "standard",
    "5k",
    "#8d6847",
    "#241b15",
  ),
  product(
    "cabinet-03",
    "Mobile Pedestal",
    "cabinet",
    990000,
    MODEL_PATHS.cabinet,
    0.84,
    0,
    0,
    "standard",
    "5k",
    "#a77d52",
    "#312319",
  ),
  product(
    "decor-01",
    "Abstract Canvas Art",
    "decor",
    990000,
    null,
    1,
    0,
    0,
    "fallback",
    "1k",
    "#ddc8a6",
    "#3a2b20",
  ),
  product(
    "decor-02",
    "Ceramic Vase Large",
    "decor",
    690000,
    null,
    1,
    0,
    0,
    "fallback",
    "1k",
    "#e3ded4",
    "#5a4a3e",
  ),
  product(
    "decor-03",
    "Sculpture Statue",
    "decor",
    1990000,
    null,
    1,
    0,
    0,
    "fallback",
    "2k",
    "#b7aea0",
    "#29241f",
  ),
  product(
    "shelf-01",
    "Wall Shelf Unit",
    "shelf",
    1590000,
    null,
    1,
    0,
    0,
    "fallback",
    "3k",
    "#9c7350",
    "#251b14",
  ),
  product(
    "shelf-02",
    "Modular Bookshelf",
    "shelf",
    4290000,
    null,
    0.95,
    0,
    0,
    "fallback",
    "3k",
    "#8b6a4f",
    "#2c221a",
  ),
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
  windowScare: {
    triggerMeshes: [],
    glassPanel: null,
    scareGroup: null,
    light: null,
    voidPlane: null,
    flashPlane: null,
    darkOverlay: null,
    screenHitbox: null,
    active: false,
    modelReady: false,
    modelFailed: false,
    startTime: 0,
    cooldownUntil: 0,
  },
  animationFrame: null,
  cosmos: {
    active: false,
    starfield: null,
    nebulaGlow: null,
    telescope: null,
    shootingStars: null,
    ambientStarLight: null,
    roomShake: 0,
    muralMesh: null,
    originalMuralMaterial: null,
    cosmosMuralMaterial: null,
  },
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
  toast: document.getElementById("roomToast"),
};

initRoom3D();

function initRoom3D() {
  if (!dom.canvas) return;

  try {
    createScene();
    createCamera();
    createRenderer();
    createScareDarkOverlay();
    createLights();
    createRoomShell();
    createControls();
    renderCategoryFilters();
    renderProductList();
    setRoomCartButtonLabel();
    bindEvents();
    resumePendingRoomCart();
    updateInspector();
    updateTotal();
    handleResize();
    animate();
    loadAiRecommendedProducts();
    setupCosmosMode();
    observeThemeChanges();
  } catch (err) {
    console.error("[Room3D] Init error:", err);
  }

  window.setTimeout(() => dom.loadingOverlay?.classList.add("is-hidden"), 550);
}

function setRoomCartButtonLabel() {
  const label = dom.buyRoomBtn?.querySelector("span");
  if (label) label.textContent = "Thêm vào giỏ";
}

function createScareDarkOverlay() {
  const stage = dom.canvas?.closest(".room3d-stage");
  if (!stage || state.windowScare.darkOverlay) return;

  const overlay = document.createElement("div");
  overlay.className = "room-scare-overlay";
  overlay.style.opacity = "0";
  stage.appendChild(overlay);
  state.windowScare.darkOverlay = overlay;
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
    preserveDrawingBuffer: true,
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

  const muralWash = new THREE.SpotLight(
    0xffd9a7,
    1.1,
    7,
    Math.PI / 5,
    0.5,
    1.5,
  );
  muralWash.position.set(1.4, 3.1, 1.7);
  muralWash.target.position.set(0.7, 1.85, -3.9);
  state.scene.add(muralWash, muralWash.target);
}

function createControls() {
  state.orbitControls = new OrbitControls(
    state.camera,
    state.renderer.domElement,
  );
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
    metalness: 0.02,
  });
  const wallMat = new THREE.MeshStandardMaterial({
    color: 0xeee6dc,
    roughness: 0.9,
  });
  const trimMat = new THREE.MeshStandardMaterial({
    color: 0x4a3425,
    roughness: 0.68,
  });

  const floor = createBox(8, 0.08, 8, floorMat, [0, -0.04, 0]);
  floor.receiveShadow = true;
  state.scene.add(floor);

  addFloorLines();

  const backWall = createBox(8, 3.25, 0.12, wallMat, [0, 1.6, -4]);
  const leftWall = createBox(0.12, 3.25, 8, wallMat, [-4, 1.6, 0]);
  const rightNib = createBox(0.12, 3.25, 2.4, wallMat, [4, 1.6, -2.8]);

  state.cosmos.walls = [backWall, leftWall, rightNib];
  state.cosmos.originalWallMaterial = wallMat;

  [backWall, leftWall, rightNib].forEach((wall) => {
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
  const lineMat = new THREE.MeshBasicMaterial({
    color: 0x8f6a45,
    transparent: true,
    opacity: 0.16,
  });
  for (let z = -3.5; z <= 3.5; z += 0.5) {
    state.scene.add(createBox(8, 0.006, 0.012, lineMat, [0, 0.006, z]));
  }
}

function createWindow(trimMat) {
  const panelWidth = 0.9425;
  const panelHeight = 1.2;
  const hingeX = -2.8425;
  const panelY = 1.95;
  const panelZ = -3.86;
  const panelCenterX = panelWidth / 2;
  const glassMat = new THREE.MeshStandardMaterial({
    color: 0xdce9ee,
    roughness: 0.28,
    transparent: true,
    opacity: 0.46,
  });

  const fixedGlass = createBox(0.86, 1.06, 0.018, glassMat, [
    -3.35,
    panelY,
    panelZ - 0.012,
  ]);
  fixedGlass.userData.type = "window-scare-trigger";
  state.windowScare.triggerMeshes.push(fixedGlass);

  const glassPanel = new THREE.Group();
  glassPanel.position.set(hingeX, panelY, panelZ);
  const glass = createBox(
    panelWidth - 0.08,
    panelHeight - 0.08,
    0.012,
    glassMat,
    [panelCenterX, 0, -0.006],
  );
  glass.userData.type = "window-scare-trigger";
  glassPanel.add(glass);
  glassPanel.add(
    createBox(panelWidth, 0.028, 0.022, trimMat, [
      panelCenterX,
      panelHeight / 2 - 0.014,
      -0.002,
    ]),
  );
  glassPanel.add(
    createBox(panelWidth, 0.028, 0.022, trimMat, [
      panelCenterX,
      -panelHeight / 2 + 0.014,
      -0.002,
    ]),
  );
  glassPanel.add(
    createBox(0.028, panelHeight, 0.022, trimMat, [0.014, 0, -0.002]),
  );
  glassPanel.add(
    createBox(0.028, panelHeight, 0.022, trimMat, [
      panelWidth - 0.014,
      0,
      -0.002,
    ]),
  );
  glassPanel.userData.closedRotationY = 0;
  state.windowScare.glassPanel = glassPanel;
  state.windowScare.triggerMeshes.push(glass);

  const scareHitbox = createBox(
    2.35,
    1.75,
    0.04,
    new THREE.MeshBasicMaterial({
      color: 0xff0000,
      transparent: true,
      opacity: 0,
      depthWrite: false,
    }),
    [-2.86, 1.95, -3.72],
  );
  scareHitbox.userData.type = "window-scare-trigger";
  scareHitbox.visible = false;
  state.windowScare.screenHitbox = scareHitbox;
  state.windowScare.triggerMeshes.push(scareHitbox);

  const framePieces = [
    createBox(1.95, 0.06, 0.08, trimMat, [-2.85, 2.58, -3.86]),
    createBox(1.95, 0.06, 0.08, trimMat, [-2.85, 1.32, -3.86]),
    createBox(0.06, 1.28, 0.08, trimMat, [-3.83, 1.95, -3.86]),
    createBox(0.06, 1.28, 0.08, trimMat, [-1.87, 1.95, -3.86]),
    createBox(0.026, 1.18, 0.052, trimMat, [-2.86, 1.95, -3.858]),
  ];
  const scare = createWindowScareActor();
  state.windowScare.scareGroup = scare.group;
  state.windowScare.light = scare.light;
  state.windowScare.voidPlane = scare.voidPlane;
  state.windowScare.flashPlane = scare.flashPlane;
  state.scene.add(
    scare.voidPlane,
    scare.flashPlane,
    scare.group,
    fixedGlass,
    glassPanel,
    scareHitbox,
    ...framePieces,
  );
}

function createWindowScareActor() {
  const group = new THREE.Group();
  group.position.set(-2.85, 1.95, -3.88);
  group.scale.setScalar(0.08);
  group.visible = false;

  const hairMat = new THREE.MeshBasicMaterial({
    color: 0x050607,
    transparent: true,
    opacity: 0.98,
    side: THREE.DoubleSide,
  });
  const faceMat = new THREE.MeshBasicMaterial({
    color: 0xd8d2c7,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const deadSkinMat = new THREE.MeshBasicMaterial({
    color: 0x8c837a,
    transparent: true,
    opacity: 0.42,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const clothMat = new THREE.MeshBasicMaterial({
    color: 0xdedbd2,
    transparent: true,
    opacity: 0.94,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const eyeMat = new THREE.MeshBasicMaterial({
    color: 0x030304,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const pupilMat = new THREE.MeshBasicMaterial({
    color: 0xff1010,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const bloodMat = new THREE.MeshBasicMaterial({
    color: 0x7d0303,
    transparent: true,
    opacity: 0.88,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const tearMat = new THREE.MeshBasicMaterial({
    color: 0x010101,
    transparent: true,
    opacity: 0.9,
    side: THREE.DoubleSide,
  });
  const teethMat = new THREE.MeshBasicMaterial({
    color: 0xf8edd8,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const redGlowMat = new THREE.MeshBasicMaterial({
    color: 0xff0505,
    transparent: true,
    opacity: 0.34,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
    side: THREE.DoubleSide,
    toneMapped: false,
  });
  const voidPlane = new THREE.Mesh(
    new THREE.PlaneGeometry(1.88, 1.26),
    new THREE.MeshBasicMaterial({
      color: 0x020203,
      transparent: true,
      opacity: 0,
      side: THREE.DoubleSide,
    }),
  );
  voidPlane.position.set(-2.85, 1.95, -3.905);
  voidPlane.visible = false;

  const flashPlane = new THREE.Mesh(
    new THREE.PlaneGeometry(1.88, 1.26),
    new THREE.MeshBasicMaterial({
      color: 0xff1a12,
      transparent: true,
      opacity: 0,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
      side: THREE.DoubleSide,
      toneMapped: false,
    }),
  );
  flashPlane.position.set(-2.85, 1.95, -3.872);
  flashPlane.visible = false;

  const bodyShape = new THREE.Shape();
  bodyShape.moveTo(-0.2, 0.04);
  bodyShape.lineTo(0.2, 0.04);
  bodyShape.lineTo(0.62, -1.12);
  bodyShape.lineTo(0.28, -1.02);
  bodyShape.lineTo(0.08, -1.16);
  bodyShape.lineTo(-0.08, -1.04);
  bodyShape.lineTo(-0.28, -1.16);
  bodyShape.lineTo(-0.62, -1.02);
  bodyShape.lineTo(-0.2, 0.04);
  const body = new THREE.Mesh(new THREE.ShapeGeometry(bodyShape), clothMat);
  body.position.set(0, -0.28, 0.004);
  group.add(body);

  const hairBack = new THREE.Mesh(new THREE.CircleGeometry(0.62, 48), hairMat);
  hairBack.position.set(0, 0.14, 0.012);
  hairBack.scale.set(0.86, 1.72, 1);
  group.add(hairBack);

  const face = new THREE.Mesh(new THREE.CircleGeometry(0.34, 48), faceMat);
  face.position.set(0, 0.2, 0.052);
  face.scale.set(0.68, 1.24, 1);
  group.add(face);

  const cheekShadow = new THREE.Mesh(
    new THREE.CircleGeometry(0.22, 32),
    deadSkinMat,
  );
  cheekShadow.position.set(0, 0.02, 0.073);
  cheekShadow.scale.set(1.15, 0.68, 1);
  group.add(cheekShadow);

  [-0.19, 0.19].forEach((x, index) => {
    const sideCurtain = new THREE.Mesh(
      new THREE.PlaneGeometry(0.19, 1.08),
      hairMat,
    );
    sideCurtain.position.set(x, -0.14, 0.091 + index * 0.004);
    sideCurtain.rotation.z = THREE.MathUtils.degToRad(x < 0 ? -8 : 8);
    group.add(sideCurtain);
  });

  for (let i = 0; i < 19; i += 1) {
    const x = -0.48 + i * 0.053;
    const length = 0.68 + (i % 7) * 0.055;
    const strand = new THREE.Mesh(
      new THREE.PlaneGeometry(0.068, length),
      hairMat,
    );
    strand.position.set(x, -0.08 - length * 0.12, 0.072 + (i % 2) * 0.006);
    strand.rotation.z = THREE.MathUtils.degToRad(-10 + i * 1.1);
    group.add(strand);
  }

  const brow = new THREE.Mesh(
    new THREE.PlaneGeometry(0.48, 0.09),
    new THREE.MeshBasicMaterial({
      color: 0x050506,
      transparent: true,
      opacity: 0.82,
      side: THREE.DoubleSide,
    }),
  );
  brow.position.set(0, 0.31, 0.082);
  brow.rotation.z = THREE.MathUtils.degToRad(-2);
  group.add(brow);

  [-0.105, 0.105].forEach((x) => {
    const socketGlow = new THREE.Mesh(
      new THREE.CircleGeometry(0.12, 28),
      redGlowMat,
    );
    socketGlow.position.set(x, 0.18, 0.088);
    socketGlow.scale.set(1.26, 0.9, 1);
    group.add(socketGlow);

    const socket = new THREE.Mesh(new THREE.CircleGeometry(0.098, 28), eyeMat);
    socket.position.set(x, 0.18, 0.095);
    socket.scale.set(1.45, 0.86, 1);
    group.add(socket);

    const pupil = new THREE.Mesh(new THREE.CircleGeometry(0.03, 18), pupilMat);
    pupil.position.set(x, 0.18, 0.112);
    pupil.scale.set(0.58, 1.35, 1);
    pupil.userData.scarePulse = true;
    group.add(pupil);

    const tear = new THREE.Mesh(new THREE.PlaneGeometry(0.028, 0.26), tearMat);
    tear.position.set(x * 0.92, 0.015, 0.104);
    tear.rotation.z = THREE.MathUtils.degToRad(x < 0 ? -4 : 4);
    group.add(tear);
  });

  const mouth = new THREE.Mesh(new THREE.CircleGeometry(0.092, 28), eyeMat);
  mouth.position.set(0, -0.075, 0.106);
  mouth.scale.set(0.78, 2.65, 1);
  group.add(mouth);

  for (let i = 0; i < 7; i += 1) {
    const x = -0.072 + i * 0.024;
    const upperTooth = createBox(
      0.016,
      0.074 - Math.abs(i - 3) * 0.005,
      0.008,
      teethMat,
      [x, -0.006, 0.118],
    );
    upperTooth.rotation.z = THREE.MathUtils.degToRad(-7 + i * 2.4);
    group.add(upperTooth);

    const lowerTooth = createBox(
      0.014,
      0.06 - Math.abs(i - 3) * 0.004,
      0.008,
      teethMat,
      [x + 0.008, -0.152, 0.118],
    );
    lowerTooth.rotation.z = THREE.MathUtils.degToRad(174 - i * 2.2);
    group.add(lowerTooth);
  }

  const blood = new THREE.Mesh(new THREE.PlaneGeometry(0.08, 0.3), bloodMat);
  blood.position.set(0.028, -0.2, 0.106);
  blood.rotation.z = THREE.MathUtils.degToRad(7);
  group.add(blood);

  [-0.15, 0.04, 0.16].forEach((x, index) => {
    const crack = new THREE.Mesh(
      new THREE.PlaneGeometry(0.018, 0.2 - index * 0.035),
      tearMat,
    );
    crack.position.set(x, 0.04 + index * 0.055, 0.108);
    crack.rotation.z = THREE.MathUtils.degToRad(index % 2 === 0 ? -24 : 19);
    group.add(crack);
  });

  const leftHand = new THREE.Mesh(new THREE.CircleGeometry(0.14, 24), faceMat);
  leftHand.position.set(-0.55, -0.18, 0.088);
  leftHand.scale.set(0.7, 1.08, 1);
  leftHand.rotation.z = THREE.MathUtils.degToRad(-36);
  group.add(leftHand);

  [-0.18, -0.06, 0.06, 0.17].forEach((offset, index) => {
    const finger = createBox(0.024, 0.34 - index * 0.025, 0.012, faceMat, [
      -0.55 + offset,
      -0.02,
      0.102,
    ]);
    finger.rotation.z = THREE.MathUtils.degToRad(-26 + index * 9);
    group.add(finger);
  });

  const rightHand = new THREE.Mesh(new THREE.CircleGeometry(0.14, 24), faceMat);
  rightHand.position.set(0.55, -0.2, 0.088);
  rightHand.scale.set(0.7, 1.08, 1);
  rightHand.rotation.z = THREE.MathUtils.degToRad(36);
  group.add(rightHand);

  [-0.17, -0.06, 0.06, 0.18].forEach((offset, index) => {
    const finger = createBox(0.024, 0.33 - index * 0.025, 0.012, faceMat, [
      0.55 + offset,
      -0.06,
      0.102,
    ]);
    finger.rotation.z = THREE.MathUtils.degToRad(26 - index * 9);
    group.add(finger);
  });

  const light = new THREE.PointLight(0xff2014, 0, 2.8, 2);
  light.position.set(-2.85, 2.06, -3.42);
  state.scene.add(light);

  loadWindowScareModel(group);
  group.userData.fallbackChildren = [...group.children];
  if (WINDOW_SCARE_MODEL_URL) {
    group.children.forEach((child) => {
      child.visible = false;
    });
    group.userData.waitingForModel = true;
  }

  return { group, light, voidPlane, flashPlane };
}

function loadWindowScareModel(group) {
  if (!WINDOW_SCARE_MODEL_URL) return;

  state.gltfLoader.load(
    WINDOW_SCARE_MODEL_URL,
    (gltf) => {
      const wrapper = new THREE.Group();
      const model = gltf.scene;
      wrapper.add(model);
      prepareObjectMeshes(wrapper);
      normalizeScareModel(wrapper);
      tintScareModel(wrapper);
      addScareModelGore(wrapper);

      group.clear();
      group.add(wrapper);
      group.userData.usingModel = true;
      group.userData.waitingForModel = false;
      state.windowScare.modelReady = true;
    },
    undefined,
    (error) => {
      console.warn("Window scare model fallback:", error);
      group.clear();
      group.userData.fallbackChildren?.forEach((child) => {
        child.visible = true;
        group.add(child);
      });
      group.userData.usingModel = false;
      group.userData.waitingForModel = false;
      state.windowScare.modelFailed = true;
    },
  );
}

function normalizeScareModel(group) {
  group.position.set(0, 0, 0);
  group.rotation.set(0, 0, 0);
  group.scale.set(1, 1, 1);
  group.updateMatrixWorld(true);

  const box = new THREE.Box3().setFromObject(group);
  if (box.isEmpty()) return;

  const center = box.getCenter(new THREE.Vector3());
  const size = box.getSize(new THREE.Vector3());
  const targetHeight = 1.14;
  const targetWidth = 0.74;
  const scale = Math.min(
    targetHeight / Math.max(size.y, 0.001),
    targetWidth / Math.max(size.x, size.z, 0.001),
  );
  const content = group.children[0] || group;
  const faceCenterY = center.y + size.y * 0.28;
  content.position.x -= center.x;
  content.position.y -= faceCenterY;
  content.position.z -= center.z;
  content.rotation.y = 0;
  group.scale.setScalar(scale);
}

function addScareModelGore(group) {
  const bloodMat = new THREE.MeshBasicMaterial({
    color: 0xa00606,
    transparent: true,
    opacity: 0.5,
    side: THREE.DoubleSide,
    depthWrite: false,
    toneMapped: false,
  });
  const darkBloodMat = new THREE.MeshBasicMaterial({
    color: 0x260101,
    transparent: true,
    opacity: 0.42,
    side: THREE.DoubleSide,
    depthWrite: false,
  });
  const glowMat = new THREE.MeshBasicMaterial({
    color: 0xff1717,
    transparent: true,
    opacity: 0.18,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
    side: THREE.DoubleSide,
    toneMapped: false,
  });

  const gore = new THREE.Group();
  gore.name = "scare-blood-face-layer";
  gore.renderOrder = 40;

  [
    [-0.11, 0.28, 0.42, 0.055, 0.24, -18, bloodMat],
    [0.07, 0.22, 0.43, 0.045, 0.18, 12, darkBloodMat],
    [0.16, 0.06, 0.42, 0.07, 0.34, 7, bloodMat],
    [-0.18, -0.02, 0.41, 0.05, 0.22, -9, darkBloodMat],
  ].forEach(([x, y, z, w, h, rot, mat]) => {
    const streak = new THREE.Mesh(new THREE.PlaneGeometry(w, h), mat);
    streak.position.set(x, y, z);
    streak.rotation.z = THREE.MathUtils.degToRad(rot);
    gore.add(streak);
  });

  [
    [-0.12, 0.34, 0.44, 0.12],
    [0.12, 0.34, 0.44, 0.12],
    [0, 0.14, 0.45, 0.18],
  ].forEach(([x, y, z, radius]) => {
    const splat = new THREE.Mesh(new THREE.CircleGeometry(radius, 22), glowMat);
    splat.position.set(x, y, z);
    splat.scale.set(1.35, 0.72, 1);
    splat.userData.scareGorePulse = true;
    gore.add(splat);
  });

  group.add(gore);
}

function tintScareModel(group) {
  group.traverse((child) => {
    if (!child.isMesh || !child.material) return;
    const patch = (material) => {
      const cloned = material.clone();
      if (cloned.color) {
        cloned.color.multiplyScalar(cloned.map ? 0.78 : 0.58);
      }
      if ("emissive" in cloned) {
        cloned.emissive = new THREE.Color(0x1d0202);
        cloned.emissiveIntensity = 0.1;
      }
      if ("roughness" in cloned) cloned.roughness = 0.92;
      if ("metalness" in cloned) cloned.metalness = 0.02;
      cloned.side = THREE.DoubleSide;
      cloned.needsUpdate = true;
      return cloned;
    };
    child.material = Array.isArray(child.material)
      ? child.material.map(patch)
      : patch(child.material);
    child.castShadow = true;
    child.receiveShadow = true;
  });
}

function createSignatureGraffiti(trimMat) {
  const frameBackMat = new THREE.MeshStandardMaterial({
    color: 0x16120f,
    roughness: 0.62,
  });
  const backingMat = new THREE.MeshStandardMaterial({
    color: 0xfaf4ec,
    roughness: 0.74,
  });
  const centerX = 0.98;
  const centerY = 1.82;
  const artWidth = 2.74;
  const artHeight = 2.06;
  const backing = createBox(
    artWidth + 0.2,
    artHeight + 0.2,
    0.035,
    backingMat,
    [centerX, centerY, -3.915],
  );
  const frame = [
    createBox(artWidth + 0.32, 0.055, 0.07, frameBackMat, [
      centerX,
      centerY + artHeight / 2 + 0.1,
      -3.875,
    ]),
    createBox(artWidth + 0.32, 0.055, 0.07, frameBackMat, [
      centerX,
      centerY - artHeight / 2 - 0.1,
      -3.875,
    ]),
    createBox(0.055, artHeight + 0.26, 0.07, frameBackMat, [
      centerX - artWidth / 2 - 0.1,
      centerY,
      -3.875,
    ]),
    createBox(0.055, artHeight + 0.26, 0.07, frameBackMat, [
      centerX + artWidth / 2 + 0.1,
      centerY,
      -3.875,
    ]),
  ];
  state.scene.add(backing, ...frame);

  const geometry = new THREE.PlaneGeometry(artWidth, artHeight);
  const fallbackMaterial = new THREE.MeshStandardMaterial({
    color: 0xf8efe4,
    roughness: 0.72,
  });
  const mural = new THREE.Mesh(geometry, fallbackMaterial);
  mural.position.set(centerX, centerY, -3.852);
  mural.castShadow = false;
  mural.receiveShadow = false;
  mural.name = "Khải Đẹp Trai signature graffiti";
  state.scene.add(mural);
  state.cosmos.muralMesh = mural;

  state.textureLoader.load(
    "/models/graffiti-3droom-mvc.svg",
    (texture) => {
      texture.colorSpace = THREE.SRGBColorSpace;
      texture.anisotropy = Math.min(
        8,
        state.renderer.capabilities.getMaxAnisotropy(),
      );
      const newMaterial = new THREE.MeshStandardMaterial({
        map: texture,
        transparent: true,
        roughness: 0.62,
        metalness: 0.02,
      });
      if (state.cosmos.active) {
        if (state.cosmos.originalMuralMaterial) {
          state.cosmos.originalMuralMaterial.dispose();
        }
        state.cosmos.originalMuralMaterial = newMaterial;
      } else {
        mural.material.dispose();
        mural.material = newMaterial;
      }
    },
    undefined,
    () => {
      const newMaterial = createCanvasGraffitiMaterial();
      if (state.cosmos.active) {
        if (state.cosmos.originalMuralMaterial) {
          state.cosmos.originalMuralMaterial.dispose();
        }
        state.cosmos.originalMuralMaterial = newMaterial;
      } else {
        mural.material.dispose();
        mural.material = newMaterial;
      }
    },
  );

  const led = createBox(artWidth + 0.04, 0.025, 0.035, trimMat, [
    centerX,
    centerY + artHeight / 2 + 0.22,
    -3.82,
  ]);
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
    metalness: 0.01,
  });
  const rug = createBox(3.25, 0.035, 2.25, rugMat, [0.8, 0.02, 0.82]);
  rug.receiveShadow = true;
  state.scene.add(rug);
}

function createFixedDecor() {
  const plant = createPlantPlaceholder(
    { id: "fixed-plant", name: "Showroom plant", category: "plant", price: 0 },
    true,
  );
  plant.position.set(3.2, 0, -3.1);
  plant.scale.setScalar(0.9);

  state.scene.add(plant);
}

function createSignatureDecor() {
  STATIC_DECOR.forEach((config) => {
    loadStaticDecorModel(config)
      .then((object) => {
        placeStaticDecor(object, config);
        if (config.fallback === "katana") {
          state.scene.add(createKatanaWallMount(config));
        }
        state.scene.add(object);
      })
      .catch((error) => {
        console.warn(
          `Room3D static decor fallback for ${config.id}. Real model is missing or failed to load. Put the GLB/scene.gltf asset in /models/rooms/decor to replace this styled fallback.`,
          error,
        );
        const fallback = createStaticDecorFallback(config);
        placeStaticDecor(fallback, config);
        state.scene.add(fallback);
      });
  });

  const katanaRim = new THREE.PointLight(0xf6fbff, 0.95, 3.1, 2);
  katanaRim.position.set(-3.12, 2.02, 0);
  state.scene.add(katanaRim);

  const katanaAccent = new THREE.SpotLight(
    0xf8fbff,
    1.35,
    3.8,
    Math.PI / 9,
    0.42,
    1.5,
  );
  katanaAccent.position.set(-2.72, 2.2, 0);
  katanaAccent.target.position.set(-3.82, 1.72, 0);
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
      (gltf) => {
        const group = new THREE.Group();
        if (config.modelPreRotation) {
          gltf.scene.rotation.set(
            THREE.MathUtils.degToRad(config.modelPreRotation[0] || 0),
            THREE.MathUtils.degToRad(config.modelPreRotation[1] || 0),
            THREE.MathUtils.degToRad(config.modelPreRotation[2] || 0),
          );
        }
        group.add(gltf.scene);
        prepareObjectMeshes(group);
        normalizeStaticDecorModel(group, config);
        if (config.fallback === "katana") {
          polishBrightKatanaMaterials(group);
        }
        applyStaticDecorMeta(group, config, false);
        group.userData.sourceUrl = modelUrl;
        resolve(group);
      },
      undefined,
      (error) => {
        const nextIndex = index + 1;
        if (nextIndex < candidates.length) {
          loadStaticDecorCandidate(config, candidates, nextIndex)
            .then(resolve)
            .catch(reject);
          return;
        }
        reject(error);
      },
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
  const heightScale = config.maxHeight
    ? config.maxHeight / Math.max(size.y, 0.001)
    : Infinity;
  const widthScale = config.maxWidth
    ? config.maxWidth / Math.max(maxWidth, 0.001)
    : Infinity;
  const fitScale = Math.min(heightScale, widthScale);
  const content = group.children[0] || group;

  content.position.x -= center.x;
  content.position.y -= box.min.y;
  content.position.z -= center.z;
  group.scale.setScalar(fitScale);
}

function placeStaticDecor(object, config) {
  object.position.set(
    config.position[0],
    config.position[1],
    config.position[2],
  );
  if (config.modelOffset) {
    object.position.x += config.modelOffset[0] || 0;
    object.position.y += config.modelOffset[1] || 0;
    object.position.z += config.modelOffset[2] || 0;
  }
  object.rotation.set(
    THREE.MathUtils.degToRad(config.rotation?.[0] || 0),
    THREE.MathUtils.degToRad(config.rotation?.[1] || 0),
    THREE.MathUtils.degToRad(config.rotation?.[2] || 0),
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
    credit: config.credit,
  };
}

function createStaticDecorFallback(config) {
  const fallback =
    config.fallback === "katana"
      ? createKatanaFallback()
      : createShenronFallback();
  normalizeStaticDecorModel(fallback, config);
  applyStaticDecorMeta(fallback, config, true);
  return fallback;
}

function createKatanaWallMount(config) {
  const group = new THREE.Group();
  const alcoveMat = new THREE.MeshStandardMaterial({
    color: 0x15100d,
    roughness: 0.86,
    metalness: 0.02,
  });
  const plaqueMat = new THREE.MeshStandardMaterial({
    color: 0x241711,
    roughness: 0.78,
    metalness: 0.04,
  });
  const trimMat = new THREE.MeshStandardMaterial({
    color: 0x6f4b2c,
    roughness: 0.64,
    metalness: 0.07,
  });
  const goldMat = new THREE.MeshStandardMaterial({
    color: 0xc49a44,
    roughness: 0.36,
    metalness: 0.42,
  });
  const shadowMat = new THREE.MeshStandardMaterial({
    color: 0x120d0a,
    roughness: 0.82,
  });
  const redMat = new THREE.MeshStandardMaterial({
    color: 0x5f1513,
    roughness: 0.74,
    metalness: 0.03,
  });

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

  [-1.38, 1.38].forEach((x) => {
    const support = createBox(0.13, 0.48, 0.13, shadowMat, [x, 0.08, 0.065]);
    const topPeg = createCylinder(0.04, 0.04, 0.24, goldMat, [x, 0.25, 0.15]);
    const lowerPeg = createCylinder(0.04, 0.04, 0.24, goldMat, [
      x,
      -0.08,
      0.15,
    ]);
    topPeg.rotation.x = Math.PI / 2;
    lowerPeg.rotation.x = Math.PI / 2;
    group.add(support, topPeg, lowerPeg);
  });

  group.position.set(
    config.position[0] - 0.11,
    config.position[1] + 0.18,
    config.position[2],
  );
  group.rotation.set(
    THREE.MathUtils.degToRad(config.rotation?.[0] || 0),
    THREE.MathUtils.degToRad(config.rotation?.[1] || 0),
    0,
  );
  group.name = "Katana wall rack";
  return group;
}

function polishBrightKatanaMaterials(object) {
  object.traverse((child) => {
    if (!child.isMesh || !child.material) return;
    const polish = (material) => {
      if (!material.isMeshStandardMaterial) return material;
      const enhanced = material.clone();
      const brightness = enhanced.color
        ? enhanced.color.r + enhanced.color.g + enhanced.color.b
        : 0;
      const hasEmissiveTexture = Boolean(enhanced.emissiveMap);
      const isBladeLike = brightness > 2.15 || hasEmissiveTexture;
      enhanced.metalness = isBladeLike
        ? 0.86
        : Math.max(enhanced.metalness ?? 0, 0.18);
      enhanced.roughness = isBladeLike
        ? 0.16
        : Math.min(enhanced.roughness ?? 0.6, 0.48);
      enhanced.emissive = isBladeLike
        ? new THREE.Color(0xdde8ff)
        : new THREE.Color(0x000000);
      enhanced.emissiveIntensity = isBladeLike
        ? hasEmissiveTexture
          ? Math.max(enhanced.emissiveIntensity ?? 0, 1.85)
          : 0.16
        : 0;
      enhanced.envMapIntensity = isBladeLike ? 2.8 : 1.15;
      if (isBladeLike) {
        enhanced.color = new THREE.Color(0xf6fbff);
      }
      enhanced.side = THREE.DoubleSide;
      enhanced.toneMapped = false;
      enhanced.needsUpdate = true;
      return enhanced;
    };

    child.material = Array.isArray(child.material)
      ? child.material.map(polish)
      : polish(child.material);
  });
}

function createShenronFallback() {
  const group = new THREE.Group();
  const jade = new THREE.MeshStandardMaterial({
    color: 0x58d6a0,
    roughness: 0.5,
    metalness: 0.12,
    emissive: 0x176145,
    emissiveIntensity: 0.045,
  });
  const belly = new THREE.MeshStandardMaterial({
    color: 0xd8c782,
    roughness: 0.62,
    metalness: 0.08,
  });
  const gold = new THREE.MeshStandardMaterial({
    color: 0xd4a93b,
    roughness: 0.5,
    metalness: 0.34,
  });
  const clawMat = new THREE.MeshStandardMaterial({
    color: 0xd6c9a4,
    roughness: 0.54,
    metalness: 0.16,
  });

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
  const body = new THREE.Mesh(
    new THREE.TubeGeometry(curve, 180, 0.086, 24, false),
    jade,
  );
  body.castShadow = true;
  body.receiveShadow = true;
  group.add(body);

  const bellyCurve = new THREE.CatmullRomCurve3(
    points.map((point, index) => {
      const next = points[Math.min(index + 1, points.length - 1)];
      const tangent = next.clone().sub(point).normalize();
      const side = new THREE.Vector3(-tangent.y, tangent.x, 0).normalize();
      return point
        .clone()
        .add(side.multiplyScalar(0.052))
        .add(new THREE.Vector3(0, -0.02, 0.018));
    }),
  );
  const bellyStrip = new THREE.Mesh(
    new THREE.TubeGeometry(bellyCurve, 180, 0.026, 10, false),
    belly,
  );
  bellyStrip.castShadow = true;
  group.add(bellyStrip);

  const head = new THREE.Group();
  head.position.copy(points[points.length - 1]);
  head.rotation.set(
    THREE.MathUtils.degToRad(-3),
    THREE.MathUtils.degToRad(-38),
    THREE.MathUtils.degToRad(4),
  );
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

  const whiskerMat = new THREE.MeshStandardMaterial({
    color: 0xf1dfae,
    roughness: 0.52,
    metalness: 0.08,
  });
  [-1, 1].forEach((side) => {
    const headOrigin = points[points.length - 1];
    const whiskerCurve = new THREE.CatmullRomCurve3([
      headOrigin.clone().add(new THREE.Vector3(0.14, -0.02, side * 0.08)),
      headOrigin.clone().add(new THREE.Vector3(0.5, 0.07, side * 0.27)),
      headOrigin.clone().add(new THREE.Vector3(0.9, -0.1, side * 0.54)),
    ]);
    const whisker = new THREE.Mesh(
      new THREE.TubeGeometry(whiskerCurve, 24, 0.008, 6, false),
      whiskerMat,
    );
    whisker.castShadow = true;
    group.add(whisker);
  });

  for (let i = 8; i < points.length - 6; i += 6) {
    const spine = createCylinder(0.018, 0.004, 0.13, gold, [
      points[i].x,
      points[i].y + 0.055,
      points[i].z,
    ]);
    spine.rotation.z = THREE.MathUtils.degToRad(18);
    group.add(spine);
  }

  [0.18, 0.4, 0.64, 0.86].forEach((t) => {
    const index = Math.floor(t * (points.length - 1));
    const point = points[index];
    const claw = createCylinder(0.018, 0.006, 0.18, clawMat, [
      point.x + 0.09,
      point.y - 0.04,
      point.z + 0.1,
    ]);
    claw.rotation.z = THREE.MathUtils.degToRad(58);
    group.add(claw);
  });

  return group;
}

function createKatanaFallback() {
  const group = new THREE.Group();
  const blade = new THREE.MeshStandardMaterial({
    color: 0xd9d9d6,
    roughness: 0.2,
    metalness: 0.86,
  });
  const edge = new THREE.MeshStandardMaterial({
    color: 0xffffff,
    roughness: 0.18,
    metalness: 0.72,
  });
  const handle = new THREE.MeshStandardMaterial({
    color: 0x171412,
    roughness: 0.62,
  });
  const wrap = new THREE.MeshStandardMaterial({
    color: 0x7c1f1f,
    roughness: 0.68,
  });
  const wood = new THREE.MeshStandardMaterial({
    color: 0x3a281f,
    roughness: 0.72,
  });
  const lacquer = new THREE.MeshStandardMaterial({
    color: 0x17100d,
    roughness: 0.5,
    metalness: 0.08,
  });
  const gold = new THREE.MeshStandardMaterial({
    color: 0xb08a3a,
    roughness: 0.42,
    metalness: 0.32,
  });
  const cord = new THREE.MeshStandardMaterial({
    color: 0x9b2722,
    roughness: 0.76,
  });

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

  [-1, -0.86, -0.72, -0.58].forEach((x) => {
    const band = createBox(0.04, 0.072, 0.064, wrap, [x, 0, 0]);
    band.position.y += 0.08;
    group.add(band);
  });

  const scabbard = createCylinder(
    0.032,
    0.032,
    1.82,
    lacquer,
    [0.24, -0.16, 0.012],
  );
  scabbard.rotation.z = Math.PI / 2;
  group.add(scabbard);
  [-0.67, 1.08].forEach((x) =>
    group.add(createBox(0.055, 0.09, 0.062, gold, [x, -0.16, 0.012])),
  );

  [-0.45, 0.15, 0.75].forEach((x, index) => {
    const tie = createBox(0.035, 0.18, 0.022, cord, [x, -0.08, 0.022]);
    tie.rotation.z = THREE.MathUtils.degToRad(index % 2 ? -18 : 18);
    group.add(tie);
  });

  const rackBack = createBox(2.5, 0.12, 0.045, wood, [0.16, -0.31, -0.02]);
  const rackLeft = createBox(0.09, 0.42, 0.095, wood, [-0.9, -0.05, -0.025]);
  const rackRight = createBox(0.09, 0.42, 0.095, wood, [1.12, -0.05, -0.025]);
  const topPegLeft = createCylinder(0.03, 0.03, 0.16, gold, [-0.9, 0.08, 0.03]);
  const topPegRight = createCylinder(
    0.03,
    0.03,
    0.16,
    gold,
    [1.12, 0.08, 0.03],
  );
  topPegLeft.rotation.x = Math.PI / 2;
  topPegRight.rotation.x = Math.PI / 2;
  const lowerPegLeft = createCylinder(
    0.03,
    0.03,
    0.16,
    gold,
    [-0.9, -0.16, 0.03],
  );
  const lowerPegRight = createCylinder(
    0.03,
    0.03,
    0.16,
    gold,
    [1.12, -0.16, 0.03],
  );
  lowerPegLeft.rotation.x = Math.PI / 2;
  lowerPegRight.rotation.x = Math.PI / 2;
  group.add(rackBack, rackLeft, rackRight);
  group.add(topPegLeft, topPegRight, lowerPegLeft, lowerPegRight);

  return group;
}

function renderCategoryFilters() {
  if (!dom.categoryFilters) return;
  const categories = [
    "all",
    ...new Set(demoProducts.map((item) => item.category)),
  ];
  dom.categoryFilters.innerHTML = categories
    .map(
      (category) => `
        <button type="button" class="category-chip${category === "all" ? " is-active" : ""}" data-category="${escapeHtml(category)}">
            ${category === "all" ? "Tất cả" : escapeHtml(category)}
        </button>
    `,
    )
    .join("");
}

function renderProductList() {
  if (!dom.productList) return;
  const products = getFilteredProducts();
  dom.productList.innerHTML = "";
  dom.libraryCount.textContent = `${products.length}/${demoProducts.length} models`;

  products.forEach((productItem) => {
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

    card
      .querySelector(".add-product-btn")
      ?.addEventListener("click", () => addProductToRoom(productItem, card));
    card
      .querySelector(".view-product-btn")
      ?.addEventListener("click", () => openProductDetail(productItem));
    dom.productList.appendChild(card);
  });
}

function getFilteredProducts() {
  const query = state.searchQuery.trim().toLowerCase();
  return demoProducts.filter((item) => {
    const matchesCategory =
      state.activeCategory === "all" || item.category === state.activeCategory;
    const matchesSearch =
      !query || `${item.name} ${item.category}`.toLowerCase().includes(query);
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
      (asset) => {
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
      (error) => {
        const nextIndex = index + 1;
        if (nextIndex < candidates.length) {
          loadModelCandidate(productItem, candidates, nextIndex)
            .then(resolve)
            .catch(reject);
          return;
        }
        reject(error);
      },
    );
  });
}

function applyProductTextureMaps(group, productItem) {
  const textureInfo = TEXTURE_SETS[productItem.model3DUrl];
  if (!textureInfo) return;

  const maps = loadTextureSet(textureInfo);
  group.traverse((child) => {
    if (!child.isMesh) return;

    const sourceMaterial = Array.isArray(child.material)
      ? child.material[0]
      : child.material;
    const material = new THREE.MeshStandardMaterial({
      color: sourceMaterial?.color?.clone?.() || new THREE.Color(0xffffff),
      map: maps.baseColor,
      normalMap: maps.normal,
      roughnessMap: maps.roughness,
      metalnessMap: maps.metallic,
      roughness: 0.72,
      metalness: 0.04,
    });

    if (maps.metallic) material.metalness = 0.18;
    child.material = material;
    child.material.needsUpdate = true;
  });
}

function loadTextureSet(textureInfo) {
  if (textureInfo.cache) return textureInfo.cache;

  const load = (url) => {
    if (!url) return null;
    const texture = state.textureLoader.load(url);
    texture.colorSpace = url.includes("basecolor")
      ? THREE.SRGBColorSpace
      : THREE.NoColorSpace;
    texture.anisotropy = Math.min(
      8,
      state.renderer.capabilities.getMaxAnisotropy(),
    );
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
  };

  textureInfo.cache = {
    baseColor: load(textureInfo.baseColor),
    normal: load(textureInfo.normal),
    roughness: load(textureInfo.roughness),
    metallic: load(textureInfo.metallic),
  };

  return textureInfo.cache;
}

function cloneModelGroup(source) {
  const clone = source.clone(true);
  clone.traverse((child) => {
    if (!child.isMesh || !child.material) return;
    child.material = Array.isArray(child.material)
      ? child.material.map((material) => material.clone())
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
    [-1.9, 0, 1.25],
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
    quantity: 1,
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
    maxWidth: 1.45,
  });
  return group;
}

function createSofaPlaceholder() {
  const group = new THREE.Group();
  const fabric = new THREE.MeshStandardMaterial({
    color: 0xc9b6a2,
    roughness: 0.9,
  });
  const seam = new THREE.MeshStandardMaterial({
    color: 0xbca792,
    roughness: 0.92,
  });
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
  const wood = new THREE.MeshStandardMaterial({
    color: 0x8b5f3d,
    roughness: 0.78,
  });
  const dark = new THREE.MeshStandardMaterial({
    color: 0x2a211a,
    roughness: 0.72,
  });
  group.add(createBox(1.55, 0.14, 0.78, wood, [0, 0.52, 0]));
  [
    [-0.62, -0.27],
    [0.62, -0.27],
    [-0.62, 0.27],
    [0.62, 0.27],
  ].forEach(([x, z]) => {
    group.add(createBox(0.08, 0.5, 0.08, dark, [x, 0.25, z]));
  });
  return group;
}

function createChairPlaceholder() {
  const group = new THREE.Group();
  const fabric = new THREE.MeshStandardMaterial({
    color: 0x9fab94,
    roughness: 0.86,
  });
  const wood = new THREE.MeshStandardMaterial({
    color: 0x5e412d,
    roughness: 0.72,
  });
  group.add(createBox(0.82, 0.18, 0.78, fabric, [0, 0.55, 0]));
  group.add(createBox(0.86, 0.9, 0.16, fabric, [0, 0.98, -0.36]));
  [
    [-0.32, -0.26],
    [0.32, -0.26],
    [-0.32, 0.26],
    [0.32, 0.26],
  ].forEach(([x, z]) => {
    group.add(createBox(0.08, 0.55, 0.08, wood, [x, 0.27, z]));
  });
  return group;
}

function createLampPlaceholder(productItem, fixed = false) {
  const group = new THREE.Group();
  const metal = new THREE.MeshStandardMaterial({
    color: 0x3a3027,
    roughness: 0.48,
    metalness: 0.28,
  });
  const shade = new THREE.MeshStandardMaterial({
    color: 0xf2c98b,
    roughness: 0.7,
    emissive: 0x503318,
    emissiveIntensity: 0.18,
  });
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
  const potMat = new THREE.MeshStandardMaterial({
    color: 0x7f4a31,
    roughness: 0.84,
    metalness: 0.02,
  });
  const potRimMat = new THREE.MeshStandardMaterial({
    color: 0xa66a42,
    roughness: 0.78,
    metalness: 0.04,
  });
  const soilMat = new THREE.MeshStandardMaterial({
    color: 0x2c1c13,
    roughness: 0.96,
  });
  const stemMat = new THREE.MeshStandardMaterial({
    color: 0x4a321f,
    roughness: 0.78,
  });
  const leafDark = new THREE.MeshStandardMaterial({
    color: 0x2f6a43,
    roughness: 0.72,
    side: THREE.DoubleSide,
  });
  const leafMid = new THREE.MeshStandardMaterial({
    color: 0x4f9661,
    roughness: 0.76,
    side: THREE.DoubleSide,
  });
  const leafLight = new THREE.MeshStandardMaterial({
    color: 0x79b46d,
    roughness: 0.68,
    emissive: 0x173b22,
    emissiveIntensity: 0.035,
    side: THREE.DoubleSide,
  });
  const veinMat = new THREE.MeshBasicMaterial({
    color: 0xd7e7b3,
    transparent: true,
    opacity: 0.46,
  });

  group.add(createCylinder(0.31, 0.22, 0.4, potMat, [0, 0.2, 0]));
  group.add(createCylinder(0.33, 0.33, 0.07, potRimMat, [0, 0.42, 0]));
  group.add(createCylinder(0.24, 0.27, 0.045, soilMat, [0, 0.455, 0]));
  group.add(createCylinder(0.28, 0.32, 0.055, potRimMat, [0, 0.045, 0]));

  const stems = [
    [0, 0.8, 0, 0, 0, 0],
    [-0.1, 0.76, 0.05, 10, 0, 14],
    [0.12, 0.82, -0.04, -8, 0, -16],
    [0.02, 0.92, 0.08, 16, 0, -6],
  ];
  stems.forEach(([x, y, z, rx, ry, rz]) => {
    const stem = createCylinder(0.018, 0.025, 0.8, stemMat, [x, y, z]);
    stem.rotation.set(
      THREE.MathUtils.degToRad(rx),
      THREE.MathUtils.degToRad(ry),
      THREE.MathUtils.degToRad(rz),
    );
    group.add(stem);
  });

  const leaves = [
    [-0.34, 1.18, 0.04, 0.34, 0.74, leafDark, -16, -26, 34],
    [0.33, 1.22, -0.06, 0.33, 0.76, leafMid, 14, 28, -35],
    [-0.16, 1.42, -0.1, 0.28, 0.66, leafLight, 22, -10, 18],
    [0.08, 1.52, 0.1, 0.3, 0.7, leafMid, -20, 12, -12],
    [0.38, 1.02, 0.12, 0.26, 0.6, leafDark, -8, 38, -48],
    [-0.38, 0.98, -0.12, 0.27, 0.62, leafMid, 12, -38, 44],
    [0, 1.3, 0.18, 0.25, 0.58, leafLight, -28, 0, 0],
    [0.18, 1.35, -0.22, 0.24, 0.56, leafDark, 30, 24, -24],
    [-0.05, 1.08, 0.22, 0.22, 0.52, leafMid, -34, -8, 8],
  ];
  leaves.forEach(([x, y, z, width, height, material, rx, ry, rz]) => {
    const leaf = createPlantLeaf(width, height, material, veinMat);
    leaf.position.set(x, y, z);
    leaf.rotation.set(
      THREE.MathUtils.degToRad(rx),
      THREE.MathUtils.degToRad(ry),
      THREE.MathUtils.degToRad(rz),
    );
    group.add(leaf);
  });

  if (productItem) applyRoomItemMeta(group, productItem, !fixed);
  return group;
}

function createPlantLeaf(width, height, material, veinMat) {
  const group = new THREE.Group();
  const shape = new THREE.Shape();
  shape.moveTo(0, height * 0.5);
  shape.bezierCurveTo(
    width * 0.54,
    height * 0.24,
    width * 0.5,
    -height * 0.3,
    0,
    -height * 0.5,
  );
  shape.bezierCurveTo(
    -width * 0.5,
    -height * 0.3,
    -width * 0.54,
    height * 0.24,
    0,
    height * 0.5,
  );

  const leaf = new THREE.Mesh(new THREE.ShapeGeometry(shape, 24), material);
  leaf.castShadow = true;
  leaf.receiveShadow = true;
  group.add(leaf);

  const vein = createBox(
    width * 0.035,
    height * 0.82,
    0.004,
    veinMat,
    [0, 0, 0.006],
  );
  vein.castShadow = false;
  vein.receiveShadow = false;
  group.add(vein);

  return group;
}

function createRugPlaceholder() {
  const group = new THREE.Group();
  const rugMat = new THREE.MeshStandardMaterial({
    color: 0xd39a69,
    roughness: 0.96,
  });
  group.add(createBox(2.35, 0.03, 1.55, rugMat, [0, 0.025, 0]));
  return group;
}

function createCabinetPlaceholder() {
  const group = new THREE.Group();
  const wood = new THREE.MeshStandardMaterial({
    color: 0x806044,
    roughness: 0.82,
  });
  const dark = new THREE.MeshStandardMaterial({
    color: 0x2b231d,
    roughness: 0.75,
  });
  group.add(createBox(1.85, 0.62, 0.42, wood, [0, 0.42, 0]));
  group.add(createBox(0.03, 0.42, 0.43, dark, [0, 0.45, 0]));
  group.add(createBox(1.6, 0.035, 0.46, dark, [0, 0.76, 0]));
  return group;
}

function createShelfPlaceholder() {
  const group = new THREE.Group();
  const wood = new THREE.MeshStandardMaterial({
    color: 0x8b684c,
    roughness: 0.82,
  });
  const dark = new THREE.MeshStandardMaterial({
    color: 0x251d17,
    roughness: 0.7,
  });
  group.add(createBox(1.2, 0.08, 0.34, wood, [0, 0.32, 0]));
  group.add(createBox(1.2, 0.08, 0.34, wood, [0, 0.82, 0]));
  group.add(createBox(1.2, 0.08, 0.34, wood, [0, 1.32, 0]));
  group.add(createBox(0.08, 1.2, 0.34, dark, [-0.62, 0.82, 0]));
  group.add(createBox(0.08, 1.2, 0.34, dark, [0.62, 0.82, 0]));
  return group;
}

function createDecorPlaceholder() {
  const group = new THREE.Group();
  const clay = new THREE.MeshStandardMaterial({
    color: 0xd7c8b6,
    roughness: 0.7,
  });
  const accent = new THREE.MeshStandardMaterial({
    color: 0x2c2520,
    roughness: 0.58,
    metalness: 0.08,
  });
  group.add(createCylinder(0.22, 0.16, 0.48, clay, [-0.18, 0.24, 0]));
  group.add(createCylinder(0.13, 0.2, 0.36, clay, [0.2, 0.18, 0.08]));
  group.add(createBox(0.56, 0.04, 0.28, accent, [0, 0.03, 0]));
  return group;
}

function createDefaultPlaceholder() {
  const group = new THREE.Group();
  const mat = new THREE.MeshStandardMaterial({
    color: 0xb9a48d,
    roughness: 0.86,
  });
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
    defaultRotationY: productItem.rotationY || 0,
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
  dom.pitchDownBtn?.addEventListener("click", () =>
    rotateSelectedAxis("x", 10),
  );
  dom.rollLeftBtn?.addEventListener("click", () =>
    rotateSelectedAxis("z", -10),
  );
  dom.rollRightBtn?.addEventListener("click", () =>
    rotateSelectedAxis("z", 10),
  );
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
  dom.libraryToggleBtn?.addEventListener("click", () =>
    setLibraryOpen(dom.page.dataset.libraryOpen !== "true"),
  );
  dom.libraryCloseBtn?.addEventListener("click", () => setLibraryOpen(false));
  dom.inspectorToggleBtn?.addEventListener("click", () =>
    setInspectorOpen(dom.page.dataset.inspectorOpen !== "true"),
  );
  dom.inspectorCloseBtn?.addEventListener("click", () =>
    setInspectorOpen(false),
  );
  dom.productSearchInput?.addEventListener("input", (event) => {
    state.searchQuery = event.target.value;
    renderProductList();
  });
  dom.categoryFilters?.addEventListener("click", (event) => {
    const button = event.target.closest("[data-category]");
    if (!button) return;
    state.activeCategory = button.dataset.category;
    dom.categoryFilters.querySelectorAll(".category-chip").forEach((chip) => {
      chip.classList.toggle("is-active", chip === button);
    });
    renderProductList();
  });
  window.addEventListener("keydown", (event) => {
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

  // Easter Egg click overrides in Cosmos Mode
  if (state.cosmos.active) {
    state.raycaster.setFromCamera(state.pointer, state.camera);

    // 1. Raycast Telescope
    if (state.cosmos.telescope) {
      const hits = state.raycaster.intersectObject(
        state.cosmos.telescope,
        true,
      );
      if (hits.length > 0) {
        triggerTelescopeEasterEgg();
        selectObject(null);
        return;
      }
    }

    // 2. Raycast Planets
    if (state.cosmos.planetGroup) {
      const aureliaHits = state.cosmos.aureliaMesh
        ? state.raycaster.intersectObject(state.cosmos.aureliaMesh, true)
        : [];
      const seleneHits = state.cosmos.seleneMesh
        ? state.raycaster.intersectObject(state.cosmos.seleneMesh, true)
        : [];

      if (aureliaHits.length > 0) {
        triggerPlanetClick("aurelia");
        selectObject(null);
        return;
      }
      if (seleneHits.length > 0) {
        triggerPlanetClick("selene");
        selectObject(null);
        return;
      }
    }
  }

  if (getWindowScareHit()) {
    triggerWindowScare();
    selectObject(null);
    return;
  }

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

function getWindowScareHit() {
  const triggers = state.windowScare.triggerMeshes;
  if (!triggers.length) return false;
  state.raycaster.setFromCamera(state.pointer, state.camera);
  const hitbox = state.windowScare.screenHitbox;
  if (hitbox) {
    hitbox.visible = true;
    const hit = state.raycaster.intersectObject(hitbox, false).length > 0;
    hitbox.visible = false;
    if (hit) return true;
  }
  return (
    state.raycaster.intersectObjects(
      triggers.filter((mesh) => mesh !== hitbox),
      true,
    ).length > 0
  );
}

function triggerWindowScare() {
  const scare = state.windowScare;
  const now = performance.now();
  if (scare.active || now < scare.cooldownUntil) return;
  if (scare.scareGroup?.userData.waitingForModel) {
    showToast("Dang tai model doa...");
    scare.cooldownUntil = now + 900;
    return;
  }

  scare.active = true;
  scare.startTime = now;
  scare.cooldownUntil = now + 3600;
  scare.controlsWereEnabled = state.orbitControls?.enabled !== false;
  if (state.orbitControls) state.orbitControls.enabled = false;
  if (scare.scareGroup) {
    scare.scareGroup.visible = true;
    scare.scareGroup.scale.setScalar(0.08);
    scare.scareGroup.position.set(-2.85, 1.95, -3.88);
    scare.scareGroup.userData.baseRotationZ = 0;
  }
  if (scare.voidPlane) scare.voidPlane.visible = true;
  if (scare.flashPlane) scare.flashPlane.visible = true;
  if (scare.darkOverlay) scare.darkOverlay.style.opacity = "0.18";
  showToast("Coi chung!");
}

function handlePointerMove(event) {
  if (!state.isDraggingObject || !state.selectedObject) return;

  setPointerFromEvent(event);
  state.raycaster.setFromCamera(state.pointer, state.camera);
  if (!state.raycaster.ray.intersectPlane(state.dragPlane, state.dragPoint))
    return;

  const target = state.dragPoint.add(state.dragOffset);
  state.selectedObject.position.x = THREE.MathUtils.clamp(
    target.x,
    -3.45,
    3.45,
  );
  state.selectedObject.position.z = THREE.MathUtils.clamp(
    target.z,
    -3.45,
    3.45,
  );
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
  const roots = state.roomItems.map((item) => item.object3D);
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
  const target =
    (state.selectedObject.userData[key] ??
      state.selectedObject.rotation[normalizedAxis]) +
    THREE.MathUtils.degToRad(degrees);
  state.selectedObject.userData[key] = target;
}

function resetSelectedRotation() {
  if (!state.selectedObject) {
    showToast("ChÆ°a cÃ³ sáº£n pháº©m nÃ o Ä‘Æ°á»£c chá»n.");
    return;
  }

  state.selectedObject.userData.targetRotationX = 0;
  state.selectedObject.userData.targetRotationY = THREE.MathUtils.degToRad(
    state.selectedObject.userData.defaultRotationY || 0,
  );
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
  state.roomItems = state.roomItems.filter((item) => item.object3D !== object);
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
    dom.deleteBtn,
  ].forEach((btn) => {
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
  const total = state.roomItems.reduce(
    (sum, item) => sum + (item.price || 0) * (item.quantity || 1),
    0,
  );
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

async function buyRoom() {
  if (!state.roomItems.length) {
    showToast("Hãy thêm ít nhất một sản phẩm vào phòng.");
    return;
  }

  const items = state.roomItems.map((item) => ({
    productId: item.productId,
    name: item.name,
    category: item.category,
    price: item.price || 0,
    quantity: item.quantity || 1,
  }));

  await addRoomItemsToCart(items, {
    button: dom.buyRoomBtn,
    saveOnLogin: true,
  });
}

async function resumePendingRoomCart() {
  const items = consumePendingRoomCart();
  if (!items.length) return;

  window.setTimeout(() => {
    showToast("Dang tiep tuc them san pham vao gio hang...");
    addRoomItemsToCart(items, { button: dom.buyRoomBtn, saveOnLogin: false });
  }, 450);
}

async function addRoomItemsToCart(items, options = {}) {
  const token = getAntiForgeryToken();
  if (!token) {
    showToast("Khong the xac thuc phien mua hang. Hay tai lai trang.");
    return false;
  }

  const button = options.button;
  button?.setAttribute("disabled", "disabled");
  showToast("Dang them san pham vao gio hang...");

  try {
    const response = await fetch("/Cart/AddRoomItems", {
      method: "POST",
      credentials: "same-origin",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        RequestVerificationToken: token,
      },
      body: JSON.stringify({ items }),
    });
    const result = await response.json().catch(() => null);

    if (response.status === 401) {
      if (options.saveOnLogin) savePendingRoomCart(items);
      const loginUrl =
        result?.loginUrl ||
        `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
      showToast("Can dang nhap de them phong 3D vao gio hang.");
      window.setTimeout(() => {
        window.location.href = loginUrl;
      }, 650);
      return false;
    }

    if (!response.ok || !result?.success) {
      showToast(result?.message || "Chua the them bo phong vao gio hang.");
      return false;
    }

    showToast(`Da them ${result.addedCount || items.length} mon vao gio hang.`);
    window.location.href = result.redirectUrl || "/Cart";
    return true;
  } catch (error) {
    console.error("Room cart add failed:", error);
    showToast("Them bo phong vao gio hang dang bi loi ket noi.");
    return false;
  } finally {
    button?.removeAttribute("disabled");
  }
}

function savePendingRoomCart(items) {
  try {
    window.sessionStorage?.setItem(
      PENDING_ROOM_CART_KEY,
      JSON.stringify(items),
    );
  } catch (error) {
    console.warn("Cannot save pending room cart:", error);
  }
}

function consumePendingRoomCart() {
  try {
    const raw = window.sessionStorage?.getItem(PENDING_ROOM_CART_KEY);
    if (!raw) return [];
    window.sessionStorage?.removeItem(PENDING_ROOM_CART_KEY);
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed : [];
  } catch (error) {
    console.warn("Cannot restore pending room cart:", error);
    return [];
  }
}

function getAntiForgeryToken() {
  return (
    document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
    ""
  );
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
    alpha: false,
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
  const floor = createBox(
    5,
    0.04,
    5,
    new THREE.MeshStandardMaterial({ color: 0xd4b58d, roughness: 0.82 }),
    [0, -0.03, 0],
  );
  floor.receiveShadow = true;
  scene.add(hemi, key, rim, floor);

  state.detailViewer = {
    scene,
    camera,
    renderer,
    controls,
    object: null,
    autoRotate: false,
  };
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

  state.detailViewer.controls.target.set(
    center.x,
    Math.max(0.55, center.y),
    center.z,
  );
  state.detailViewer.camera.position.set(
    center.x + distance,
    Math.max(1.25, center.y + maxSize * 0.55),
    center.z + distance,
  );
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
  animateCameraTo(
    position,
    new THREE.Vector3(center.x, Math.max(0.7, center.y), center.z),
  );
}

function animateCameraTo(position, target) {
  state.cameraAnimation = {
    startTime: performance.now(),
    duration: 700,
    fromPosition: state.camera.position.clone(),
    toPosition: position,
    fromTarget: state.orbitControls.target.clone(),
    toTarget: target,
  };
}

function animate() {
  state.animationFrame = requestAnimationFrame(animate);
  state.orbitControls?.update();
  animateCamera();
  animateWindowScare();
  animateSpawnedItems();
  animateRotations();
  animateCosmosRoom();
  updateSelectionBox();

  // Immersive camera shake in response to planet clicking/destruction
  let originalCamPos = null;
  let originalControlsTarget = null;
  if (state.cosmos && state.cosmos.active && state.cosmos.roomShake > 0.001) {
    // Save original camera properties before applying temporary offset
    originalCamPos = state.camera.position.clone();
    originalControlsTarget = state.orbitControls
      ? state.orbitControls.target.clone()
      : null;

    const shake = state.cosmos.roomShake;
    const offsetX = (Math.random() - 0.5) * shake;
    const offsetY = (Math.random() - 0.5) * shake;
    const offsetZ = (Math.random() - 0.5) * shake;

    state.camera.position.x += offsetX;
    state.camera.position.y += offsetY;
    state.camera.position.z += offsetZ;

    if (state.orbitControls) {
      state.orbitControls.target.x += offsetX;
      state.orbitControls.target.y += offsetY;
      state.orbitControls.target.z += offsetZ;
    }

    // Exponential decay of room shake energy
    state.cosmos.roomShake *= 0.91;
    if (state.cosmos.roomShake < 0.001) {
      state.cosmos.roomShake = 0;
    }
  }

  state.renderer.render(state.scene, state.camera);

  // Restore original camera properties immediately after rendering to avoid drift in user controls
  if (originalCamPos) {
    state.camera.position.copy(originalCamPos);
  }
  if (originalControlsTarget && state.orbitControls) {
    state.orbitControls.target.copy(originalControlsTarget);
  }

  renderDetailViewer();
}

function renderDetailViewer() {
  if (!state.detailViewer || dom.page.dataset.detailOpen !== "true") return;

  if (state.detailViewer.object && state.detailViewer.autoRotate) {
    state.detailViewer.object.rotation.y += 0.0025;
  }

  state.detailViewer.controls.update();
  state.detailViewer.renderer.render(
    state.detailViewer.scene,
    state.detailViewer.camera,
  );
}

function animateCamera() {
  if (!state.cameraAnimation) return;

  const elapsed = Math.min(
    (performance.now() - state.cameraAnimation.startTime) /
      state.cameraAnimation.duration,
    1,
  );
  const ease = 1 - Math.pow(1 - elapsed, 3);
  state.camera.position.lerpVectors(
    state.cameraAnimation.fromPosition,
    state.cameraAnimation.toPosition,
    ease,
  );
  state.orbitControls.target.lerpVectors(
    state.cameraAnimation.fromTarget,
    state.cameraAnimation.toTarget,
    ease,
  );

  if (elapsed >= 1) {
    state.cameraAnimation = null;
  }
}

function animateWindowScare() {
  const scare = state.windowScare;
  const panel = scare.glassPanel;
  const actor = scare.scareGroup;
  if (!panel || !actor) return;

  if (!scare.active) {
    const targetRotY = state.cosmos.active
      ? -Math.PI / 2.5
      : panel.userData.closedRotationY || 0;
    panel.rotation.y = THREE.MathUtils.lerp(panel.rotation.y, targetRotY, 0.12);
    if (scare.darkOverlay) {
      const currentOpacity =
        Number.parseFloat(scare.darkOverlay.style.opacity || "0") || 0;
      scare.darkOverlay.style.opacity = `${THREE.MathUtils.lerp(currentOpacity, 0, 0.18)}`;
    }
    if (scare.light)
      scare.light.intensity = THREE.MathUtils.lerp(
        scare.light.intensity,
        0,
        0.16,
      );
    if (scare.voidPlane?.material) {
      scare.voidPlane.material.opacity = THREE.MathUtils.lerp(
        scare.voidPlane.material.opacity,
        0,
        0.16,
      );
      scare.voidPlane.visible = scare.voidPlane.material.opacity > 0.01;
    }
    if (scare.flashPlane?.material) {
      scare.flashPlane.material.opacity = THREE.MathUtils.lerp(
        scare.flashPlane.material.opacity,
        0,
        0.24,
      );
      scare.flashPlane.visible = scare.flashPlane.material.opacity > 0.01;
    }
    return;
  }

  const elapsed = performance.now() - scare.startTime;
  const progress = THREE.MathUtils.clamp(elapsed / 2800, 0, 1);
  const dread = easeOutCubic(THREE.MathUtils.clamp(progress / 0.05, 0, 1));
  const reveal = progress > 0.018 ? 1 : 0;
  const snap = easeOutCubic(
    THREE.MathUtils.clamp((progress - 0.018) / 0.04, 0, 1),
  );
  const retreat = easeOutCubic(
    THREE.MathUtils.clamp((progress - 0.76) / 0.16, 0, 1),
  );
  const hold = 1 - retreat;
  const presence = Math.max(0, reveal * hold);
  const closePresence = Math.max(0, snap * hold);
  const tension = dread * (1 - closePresence);
  const holdJitter = closePresence > 0.94 && retreat < 0.05 ? 0.018 : 0;
  const shake =
    Math.sin(elapsed * 0.055) * 0.012 * (1 - closePresence) * (1 - retreat);
  const doorOpen = easeOutCubic(THREE.MathUtils.clamp(progress / 0.08, 0, 1));

  if (scare.darkOverlay) {
    const flicker =
      Math.max(0, Math.sin(elapsed * 0.018) * Math.sin(elapsed * 0.073)) * 0.1;
    scare.darkOverlay.style.opacity = `${Math.min(0.94, 0.16 + dread * 0.5 + presence * 0.16 + closePresence * 0.18 + flicker)}`;
  }

  panel.rotation.y = -1.02 * doorOpen * (1 - retreat * 0.72) + shake;
  actor.visible = presence > 0.01;
  const start = new THREE.Vector3(-2.85, 1.95, -3.88);
  const doorway = new THREE.Vector3(
    -2.85 + Math.sin(elapsed * 0.014) * 0.055 * presence,
    1.91 + Math.sin(elapsed * 0.019) * 0.035 * presence - tension * 0.04,
    -3.14,
  );
  const cameraForward = new THREE.Vector3();
  state.camera.getWorldDirection(cameraForward);
  const cameraRight = new THREE.Vector3()
    .crossVectors(cameraForward, state.camera.up)
    .normalize();
  const cameraUp = new THREE.Vector3()
    .crossVectors(cameraRight, cameraForward)
    .normalize();
  const isLoadedModel = actor.userData.usingModel === true;
  const pulse =
    Math.sin(elapsed * 0.035) * (0.02 + closePresence * 0.028) * presence;
  const scale =
    0.08 +
    presence * (0.92 + closePresence * (isLoadedModel ? 1.65 : 3.05)) +
    pulse;
  const closeDistance = isLoadedModel ? 1.05 : 0.82;
  const faceCenterOffset = isLoadedModel ? 0 : 0.46 + closePresence * 0.2;
  const closeTarget = state.camera.position
    .clone()
    .add(cameraForward.clone().multiplyScalar(closeDistance))
    .add(cameraUp.clone().multiplyScalar(-faceCenterOffset));
  actor.position.copy(
    start.lerp(closeTarget, Math.max(reveal * 0.82, closePresence)),
  );
  actor.scale.set(
    scale,
    scale * (1 + 0.22 * presence + 0.22 * closePresence),
    scale,
  );
  actor.lookAt(state.camera.position);
  actor.rotateZ(closePresence > 0.2 ? 0 : actor.userData.baseRotationZ || 0);
  actor.traverse((child) => {
    if (child.userData?.scarePulse) {
      const eyePulse =
        0.9 +
        Math.max(0, Math.sin(elapsed * 0.024)) * (0.65 + closePresence * 0.7);
      child.scale.set(0.7 * eyePulse, 1.2 * eyePulse, 1);
    }
    if (child.userData?.scareGorePulse) {
      const gorePulse =
        1 +
        Math.max(0, Math.sin(elapsed * 0.045)) * (0.25 + closePresence * 0.35);
      child.scale.set(1.35 * gorePulse, 0.72 * gorePulse, 1);
      child.material.opacity = Math.min(
        0.38,
        0.12 + closePresence * 0.16 + gorePulse * 0.04,
      );
    }
  });

  if (scare.light) {
    scare.light.position
      .copy(actor.position)
      .add(new THREE.Vector3(0, 0.12, 0.18));
    scare.light.intensity =
      0.12 +
      dread * 0.7 +
      presence * (2.4 + closePresence * 9.2 + Math.sin(elapsed * 0.041) * 1.25);
  }

  if (scare.voidPlane?.material) {
    scare.voidPlane.visible = true;
    scare.voidPlane.material.opacity = 0.22 + dread * 0.38 + presence * 0.4;
  }

  if (scare.flashPlane?.material) {
    scare.flashPlane.visible = true;
    const flash = Math.max(0, Math.sin(progress * Math.PI * 5)) * presence;
    scare.flashPlane.material.opacity = Math.min(
      0.44,
      flash * 0.12 + closePresence * 0.2,
    );
  }

  if (progress >= 1) {
    scare.active = false;
    actor.visible = false;
    actor.scale.setScalar(0.08);
    if (scare.light) scare.light.intensity = 0;
    if (scare.voidPlane?.material) {
      scare.voidPlane.material.opacity = 0;
      scare.voidPlane.visible = false;
    }
    if (scare.flashPlane?.material) {
      scare.flashPlane.material.opacity = 0;
      scare.flashPlane.visible = false;
    }
    if (scare.darkOverlay) scare.darkOverlay.style.opacity = "0";
    if (state.orbitControls)
      state.orbitControls.enabled = scare.controlsWereEnabled !== false;
  }
}

function easeOutCubic(value) {
  return 1 - Math.pow(1 - value, 3);
}

function animateSpawnedItems() {
  const now = performance.now();
  state.roomItems.forEach((item) => {
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
  state.roomItems.forEach((item) => {
    const object = item.object3D;
    ["x", "y", "z"].forEach((axis) => {
      const key = `targetRotation${axis.toUpperCase()}`;
      if (object.userData[key] === undefined) return;
      object.rotation[axis] = THREE.MathUtils.lerp(
        object.rotation[axis],
        object.userData[key],
        0.18,
      );
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
  const radius = Math.max(
    0.55,
    Math.min(1.85, Math.max(size.x, size.z) * 0.62),
  );
  const halo = new THREE.Mesh(
    new THREE.RingGeometry(radius * 0.82, radius, 72),
    new THREE.MeshBasicMaterial({
      color: 0xff9c3f,
      transparent: true,
      opacity: 0.42,
      depthWrite: false,
      side: THREE.DoubleSide,
    }),
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
  const mesh = new THREE.Mesh(
    new THREE.BoxGeometry(width, height, depth),
    material,
  );
  mesh.position.set(position[0], position[1], position[2]);
  mesh.castShadow = true;
  mesh.receiveShadow = true;
  return mesh;
}

function createCylinder(
  radiusTop,
  radiusBottom,
  height,
  material,
  position = [0, 0, 0],
) {
  const mesh = new THREE.Mesh(
    new THREE.CylinderGeometry(radiusTop, radiusBottom, height, 32),
    material,
  );
  mesh.position.set(position[0], position[1], position[2]);
  mesh.castShadow = true;
  mesh.receiveShadow = true;
  return mesh;
}

function prepareObjectMeshes(object) {
  object.traverse((child) => {
    if (!child.isMesh) return;
    child.castShadow = true;
    child.receiveShadow = true;
    if (child.material) {
      child.material.needsUpdate = true;
    }
  });
}

function disposeObject(object) {
  object.traverse((child) => {
    if (!child.isMesh) return;
    child.geometry?.dispose();
    if (Array.isArray(child.material)) {
      child.material.forEach((material) => material.dispose?.());
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
    thumb: `/models/demo-products/sharetextures/${key}/${fileName}-preview.webp`,
  };
}

function textureSet(key, prefix, resolution = "1K") {
  const root = `/models/demo-products/sharetextures/${key}`;
  return {
    baseColor: `${root}/${prefix}_basecolor-${resolution}.png`,
    normal: `${root}/${prefix}_normal-${resolution}.png`,
    roughness: `${root}/${prefix}_roughness-${resolution}.png`,
    metallic: `${root}/${prefix}_metallic-${resolution}.png`,
    cache: null,
  };
}

function product(
  id,
  name,
  category,
  price,
  model3DUrl,
  thumbnailOrScale,
  scaleOrOffsetY,
  offsetYOrRotationY,
  rotationYOrQualityTier,
  qualityTierOrTriangles,
  trianglesOrBg,
  bgOrFg,
  fgMaybe,
) {
  const hasCustomThumbnail =
    typeof thumbnailOrScale === "string" || thumbnailOrScale === null;
  const thumbnail = hasCustomThumbnail ? thumbnailOrScale : null;
  const scale = hasCustomThumbnail ? scaleOrOffsetY : thumbnailOrScale;
  const offsetY = hasCustomThumbnail ? offsetYOrRotationY : scaleOrOffsetY;
  const rotationY = hasCustomThumbnail
    ? rotationYOrQualityTier
    : offsetYOrRotationY;
  const qualityTier = hasCustomThumbnail
    ? qualityTierOrTriangles
    : rotationYOrQualityTier;
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
    triangles,
  };
}

function withModelCandidates(productItem, candidates) {
  return {
    ...productItem,
    model3DUrlCandidates: candidates,
  };
}

function categoryIcon(category) {
  const icon =
    {
      sofa: "bi-lamp",
      table: "bi-grid-3x3-gap",
      chair: "bi-easel",
      lamp: "bi-lightbulb",
      plant: "bi-flower1",
      rug: "bi-border-style",
      cabinet: "bi-collection",
      decor: "bi-stars",
      dragon: "bi-stars",
      shelf: "bi-bookshelf",
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

async function loadAiRecommendedProducts() {
  const urlParams = new URLSearchParams(window.location.search);
  if (urlParams.get("source") !== "ai") return;

  try {
    const response = await fetch("/Ai/Room3DSelection");
    if (!response.ok) return;

    const data = await response.json();
    if (!data || !data.products || !data.products.length) return;

    showToast("Đang bày trí các sản phẩm gợi ý từ AI...");

    for (const apiProduct of data.products) {
      const demoItem = demoProducts.find((item) => {
        if (apiProduct.roomProductId) {
          return item.id === apiProduct.roomProductId;
        }

        return (
          item.name.trim().toLowerCase() ===
          apiProduct.name.trim().toLowerCase()
        );
      });

      if (demoItem) {
        // Wait for the product to load & spawn before moving to the next
        await addProductToRoom(demoItem, null);
      } else {
        console.warn(
          `Could not find demoProduct matching AI recommended product: ${apiProduct.name}`,
        );
      }
    }

    showToast("Đã bày trí xong toàn bộ bản phối gợi ý từ AI!");
  } catch (err) {
    console.error("Error loading AI recommended products in 3D room:", err);
  }
}

/* ─── Cosmos Mode for Room 3D ─── */

/* ─── Cosmos Mode for Room 3D ─── */

function isCosmosMode() {
  return document.documentElement.getAttribute("data-scene-mode") === "cosmos";
}

function observeThemeChanges() {
  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      if (mutation.attributeName === "data-scene-mode") {
        const isCosmos = isCosmosMode();
        if (isCosmos && !state.cosmos.active) {
          setupCosmosMode();
        } else if (!isCosmos && state.cosmos.active) {
          teardownCosmosMode();
        }
      }
    });
  });
  observer.observe(document.documentElement, { attributes: true });
}

function applyVanGoghWalls() {
  if (!state.cosmos.walls || state.cosmos.walls.length === 0) return;

  if (!state.cosmos.vanGoghWallMaterial) {
    const texture = createVanGoghStarryNightTexture();
    state.cosmos.vanGoghWallMaterial = new THREE.MeshStandardMaterial({
      map: texture,
      roughness: 0.85, // matte impasto oil canvas feel
      metalness: 0.05,
    });
  }

  state.cosmos.walls.forEach((wall) => {
    wall.material = state.cosmos.vanGoghWallMaterial;
    wall.material.needsUpdate = true;
  });
}

function applyCosmosMuralTheme() {
  if (!state.cosmos.muralMesh) return;

  state.textureLoader.load("/models/img-3d-room-mode-theme.svg", (texture) => {
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = Math.min(
      8,
      state.renderer.capabilities.getMaxAnisotropy(),
    );

    if (state.cosmos.active && state.cosmos.muralMesh) {
      if (!state.cosmos.originalMuralMaterial) {
        state.cosmos.originalMuralMaterial = state.cosmos.muralMesh.material;
      }

      if (state.cosmos.cosmosMuralMaterial) {
        if (state.cosmos.cosmosMuralMaterial.map) {
          state.cosmos.cosmosMuralMaterial.map.dispose();
        }
        state.cosmos.cosmosMuralMaterial.dispose();
      }

      state.cosmos.cosmosMuralMaterial = new THREE.MeshStandardMaterial({
        map: texture,
        transparent: true,
        roughness: 0.62,
        metalness: 0.02,
      });

      state.cosmos.muralMesh.material = state.cosmos.cosmosMuralMaterial;
      state.cosmos.muralMesh.material.needsUpdate = true;
    } else {
      texture.dispose();
    }
  });
}

function restoreOriginalMuralTheme() {
  if (state.cosmos.muralMesh && state.cosmos.originalMuralMaterial) {
    state.cosmos.muralMesh.material = state.cosmos.originalMuralMaterial;
    state.cosmos.muralMesh.material.needsUpdate = true;

    if (state.cosmos.cosmosMuralMaterial) {
      if (state.cosmos.cosmosMuralMaterial.map) {
        state.cosmos.cosmosMuralMaterial.map.dispose();
      }
      state.cosmos.cosmosMuralMaterial.dispose();
      state.cosmos.cosmosMuralMaterial = null;
    }
  }
}

function setupCosmosMode() {
  if (!isCosmosMode()) return;
  state.cosmos.active = true;

  // Darken scene background to deep space
  state.scene.background = new THREE.Color(0x050512);
  state.scene.fog = new THREE.Fog(0x050512, 20, 45);

  // Add very cheap global Prussian blue ambient starlight
  state.cosmos.globalAmbientLight = new THREE.AmbientLight(0x0a0f26, 0.45);
  state.scene.add(state.cosmos.globalAmbientLight);

  // Add localized blue ambient shimmer point light (optimized range & falloff)
  state.cosmos.ambientStarLight = new THREE.PointLight(0x4b3fa3, 0.65, 12, 1.8);
  state.cosmos.ambientStarLight.position.set(0, 4.2, 0);
  state.scene.add(state.cosmos.ambientStarLight);

  // Add secondary warm golden point starlight from the window (optimized range & falloff)
  state.cosmos.ambientGoldLight = new THREE.PointLight(0xffca28, 0.75, 10, 1.8);
  state.cosmos.ambientGoldLight.position.set(2.5, 2.0, -2.0);
  state.scene.add(state.cosmos.ambientGoldLight);

  // Apply Van Gogh "Starry Night" painting to the walls
  applyVanGoghWalls();

  // Build the space skybox panoramic nebula sphere surrounding the room
  createCosmosNebulaSkybox();

  // Place the detailed banded ringed planet (Saturn-like) visible from the window
  createBackgroundPlanet();

  // Build the Cosmic Swirl Nebula matching the painting
  createCosmicSwirlNebula();

  // Build the starfield spheres (layered for depth and counter-rotating)
  createCosmosStarfields();

  // Build floating stardust particles
  createStardustParticles();

  // Build shooting star particles
  createShootingStars();

  // Place the exquisite luxury telescope next to the window pointing out
  createTelescope();

  // Swap wall signature painting to Cosmos theme image
  applyCosmosMuralTheme();
}

function teardownCosmosMode() {
  state.cosmos.active = false;

  // Clean up telescope Easter Egg overlay if open
  closeTelescopeEasterEgg();

  // Restore original background and fog
  state.scene.background = new THREE.Color(0xf2ece4);
  state.scene.fog = new THREE.Fog(0xf2ece4, 12, 24);

  // Remove ambient star lights
  if (state.cosmos.globalAmbientLight) {
    state.scene.remove(state.cosmos.globalAmbientLight);
    state.cosmos.globalAmbientLight = null;
  }
  if (state.cosmos.ambientStarLight) {
    state.scene.remove(state.cosmos.ambientStarLight);
    state.cosmos.ambientStarLight = null;
  }
  if (state.cosmos.ambientGoldLight) {
    state.scene.remove(state.cosmos.ambientGoldLight);
    state.cosmos.ambientGoldLight = null;
  }

  // Remove skybox
  if (state.cosmos.skybox) {
    state.scene.remove(state.cosmos.skybox);
    state.cosmos.skybox.geometry.dispose();
    state.cosmos.skybox.material.map.dispose();
    state.cosmos.skybox.material.dispose();
    state.cosmos.skybox = null;
  }

  // Remove starfields
  if (state.cosmos.starfield1) {
    state.scene.remove(state.cosmos.starfield1);
    state.cosmos.starfield1.geometry.dispose();
    state.cosmos.starfield1.material.dispose();
    state.cosmos.starfield1 = null;
  }
  if (state.cosmos.starfield2) {
    state.scene.remove(state.cosmos.starfield2);
    state.cosmos.starfield2.geometry.dispose();
    state.cosmos.starfield2.material.dispose();
    state.cosmos.starfield2 = null;
  }

  // Remove Cosmic Swirl Nebula
  if (state.cosmos.cosmicSwirl) {
    state.scene.remove(state.cosmos.cosmicSwirl);
    state.cosmos.cosmicSwirl.geometry.dispose();
    state.cosmos.cosmicSwirl.material.map.dispose();
    state.cosmos.cosmicSwirl.material.dispose();
    state.cosmos.cosmicSwirl = null;
  }

  // Remove Stardust
  if (state.cosmos.stardust) {
    state.scene.remove(state.cosmos.stardust);
    state.cosmos.stardust.geometry.dispose();
    state.cosmos.stardust.material.dispose();
    state.cosmos.stardust = null;
    state.cosmos.stardustSpeeds = null;
  }

  // Remove planet
  if (state.cosmos.planetGroup) {
    state.scene.remove(state.cosmos.planetGroup);
    state.cosmos.planetGroup.traverse((child) => {
      if (child.isMesh) {
        child.geometry.dispose();
        if (child.material.map) child.material.map.dispose();
        child.material.dispose();
      }
    });
    state.cosmos.planetGroup = null;
    state.cosmos.aureliaGlow = null;
    state.cosmos.ignisGlow = null;
    state.cosmos.caelumGlow = null;
    state.cosmos.seleneGlow = null;
    state.cosmos.aureliaMesh = null;
    state.cosmos.ignisMesh = null;
    state.cosmos.caelumMesh = null;
    state.cosmos.seleneMesh = null;
  }
  if (state.cosmos.planetLight) {
    state.scene.remove(state.cosmos.planetLight);
    state.cosmos.planetLight = null;
  }

  // Remove shooting stars
  if (state.cosmos.shootingStars) {
    state.scene.remove(state.cosmos.shootingStars);
    state.cosmos.shootingStars.children.forEach((star) => {
      star.geometry.dispose();
      star.material.dispose();
    });
    state.cosmos.shootingStars = null;
  }

  // Remove telescope
  if (state.cosmos.telescope) {
    state.scene.remove(state.cosmos.telescope);
    state.cosmos.telescope.traverse((child) => {
      if (child.isMesh) {
        child.geometry.dispose();
        if (child.material.map) child.material.map.dispose();
        child.material.dispose();
      }
    });
    state.cosmos.telescope = null;
  }

  // Restore original wall material
  if (state.cosmos.walls && state.cosmos.originalWallMaterial) {
    state.cosmos.walls.forEach((wall) => {
      wall.material = state.cosmos.originalWallMaterial;
    });
  }

  // Restore original wall painting
  restoreOriginalMuralTheme();
}

function createVanGoghStarryNightTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  // Scale coordinate system to allow 2048x1024 logic to draw flawlessly at 1024x512
  ctx.scale(0.5, 0.5);
  const logicalWidth = 2048;
  const logicalHeight = 1024;

  // Sky Background Gradient: Dark Prussian Blue, Deep Violet and Saturated Ultramarine
  const skyGrad = ctx.createLinearGradient(0, 0, 0, logicalHeight);
  skyGrad.addColorStop(0, "#030812");
  skyGrad.addColorStop(0.2, "#081326");
  skyGrad.addColorStop(0.45, "#0f2347");
  skyGrad.addColorStop(0.7, "#1a396b");
  skyGrad.addColorStop(0.9, "#091730");
  skyGrad.addColorStop(1, "#020710");
  ctx.fillStyle = skyGrad;
  ctx.fillRect(0, 0, logicalWidth, logicalHeight);

  const stars = [
    { x: 220, y: 180, r: 26, c: "#ffd54f" },
    { x: 500, y: 320, r: 22, c: "#ffe082" },
    { x: 740, y: 160, r: 30, c: "#ffca28" },
    { x: 1040, y: 360, r: 20, c: "#ffd54f" },
    { x: 1220, y: 180, r: 34, c: "#ffeb3b" },
    { x: 1480, y: 260, r: 24, c: "#ffd54f" },
    { x: 1650, y: 460, r: 28, c: "#ffca28" },
    { x: 420, y: 520, r: 18, c: "#ffe082" },
  ];

  // Helper: draw textured brush strokes guided by vector flow fields
  function drawFlowStroke(
    startX,
    startY,
    baseColorHex,
    strokeWidth,
    length,
    opacityMultiplier = 1.0,
  ) {
    ctx.save();
    ctx.lineCap = "round";

    const hex = baseColorHex.replace("#", "");
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);

    ctx.beginPath();
    let cx = startX;
    let cy = startY;
    ctx.moveTo(cx, cy);

    const steps = Math.max(3, Math.floor(length / 8));
    for (let i = 0; i < steps; i++) {
      // Flow field calculation
      // Base horizontal winding breeze flow
      let angle = Math.sin((cx / logicalWidth) * Math.PI * 3.4) * 0.38 + 0.04;

      // Core Swirling Vortex influence
      const vx = logicalWidth * 0.52;
      const vy = logicalHeight * 0.45;
      const vdx = cx - vx;
      const vdy = cy - vy;
      const vdist = Math.sqrt(vdx * vdx + vdy * vdy);
      if (vdist < 460) {
        const vortexAngle = Math.atan2(vdy, vdx) + Math.PI / 1.72;
        const weight = Math.exp(-vdist / 170) * 0.88;
        angle = (1 - weight) * angle + weight * vortexAngle;
      }

      // Stars local swirl aura influence
      stars.forEach((star) => {
        const sdx = cx - star.x;
        const sdy = cy - star.y;
        const sdist = Math.sqrt(sdx * sdx + sdy * sdy);
        const limit = star.r * 3.4;
        if (sdist < limit) {
          const starAngle = Math.atan2(sdy, sdx) + Math.PI / 1.78;
          const weight = Math.pow(1.0 - sdist / limit, 1.4) * 0.96;
          angle = (1 - weight) * angle + weight * starAngle;
        }
      });

      // Step forward along flow angle
      cx += Math.cos(angle) * 7.5;
      cy += Math.sin(angle) * 7.5;
      ctx.lineTo(cx, cy);
    }

    const numSubStrokes = 3;
    for (let j = 0; j < numSubStrokes; j++) {
      const dr = Math.floor((Math.random() - 0.5) * 26);
      const dg = Math.floor((Math.random() - 0.5) * 26);
      const db = Math.floor((Math.random() - 0.5) * 26);
      const cr = Math.min(255, Math.max(0, r + dr));
      const cg = Math.min(255, Math.max(0, g + dg));
      const cb = Math.min(255, Math.max(0, b + db));
      const op = (0.35 + Math.random() * 0.4) * opacityMultiplier;

      // Offset translation for organic clumped feel
      const tx = (Math.random() - 0.5) * (strokeWidth * 0.28);
      const ty = (Math.random() - 0.5) * (strokeWidth * 0.28);

      // Layer A: 3D Impasto Base Shadow (Creates the physical paint height)
      ctx.save();
      ctx.translate(tx + 1.2, ty + 1.4);
      ctx.strokeStyle = "rgba(1, 4, 12, 0.28)";
      ctx.lineWidth = strokeWidth * (0.55 + Math.random() * 0.5) * 1.15;
      ctx.stroke();
      ctx.restore();

      // Layer B: Core Paint Body
      ctx.save();
      ctx.translate(tx, ty);
      ctx.strokeStyle = `rgba(${cr}, ${cg}, ${cb}, ${op})`;
      ctx.lineWidth = strokeWidth * (0.45 + Math.random() * 0.55);
      ctx.stroke();
      ctx.restore();

      // Layer C: Dynamic Ridge Highlight (Specular paint shine)
      ctx.save();
      ctx.translate(tx - 0.8, ty - 0.8);
      const hr = Math.min(255, cr + 75);
      const hg = Math.min(255, cg + 75);
      const hb = Math.min(255, cb + 55);
      ctx.strokeStyle = `rgba(${hr}, ${hg}, ${hb}, ${op * 0.65})`;
      ctx.lineWidth = strokeWidth * (0.45 + Math.random() * 0.55) * 0.38;
      ctx.stroke();
      ctx.restore();
    }
    ctx.restore();
  }

  // 1. Draw backdrop flow strokes (optimized density & sizes for speed)
  const skyColors = [
    "#09152b",
    "#0f2347",
    "#17386d",
    "#265394",
    "#356fa9",
    "#11264c",
    "#050b17",
  ];
  for (let y = 30; y < logicalHeight - 180; y += 38) {
    for (let x = -50; x < logicalWidth + 100; x += 75) {
      const col = skyColors[Math.floor(Math.random() * skyColors.length)];
      const size = 13 + Math.random() * 8;
      const len = 50 + Math.random() * 40;
      drawFlowStroke(x, y, col, size, len, 0.75);
    }
  }

  // 2. Draw signature majestic Swirling Sky Vortex currents (optimized step size)
  const swirlColors = [
    "#fff9c4",
    "#fff59d",
    "#ffe082",
    "#ffca28",
    "#ffd54f",
    "#e1f5fe",
    "#b3e5fc",
    "#ffffff",
  ];
  for (let path = 0; path < 7; path++) {
    const pathOffset = path * 16 - 48;
    const strokeW = 16 - path * 1.2;
    const colorSet =
      path % 2 === 0 ? swirlColors : ["#ffffff", "#e1f5fe", "#fff9c4"];

    for (let t = 0; t < 1.0; t += 0.005) {
      const x = t * (logicalWidth + 240) - 120;
      let y = 470 + Math.sin(t * Math.PI * 2.8) * 150 + pathOffset;

      // Vortex spiral core distortion
      if (t > 0.38 && t < 0.62) {
        const spiralT = (t - 0.38) / 0.24;
        const spiralAngle = spiralT * Math.PI * 3.8;
        const spiralRad = Math.sin(spiralT * Math.PI) * 125;
        y += Math.sin(spiralAngle) * spiralRad;
      }

      const col = colorSet[Math.floor(Math.random() * colorSet.length)];
      if (Math.floor(t * 1200) % 4 !== 0) {
        drawFlowStroke(x, y, col, strokeW, 40, 0.95);
      }
    }
  }

  // 3. Draw Stars with beautiful concentric flow halo rings
  stars.forEach((star) => {
    const radGrad = ctx.createRadialGradient(
      star.x,
      star.y,
      1,
      star.x,
      star.y,
      star.r * 1.6,
    );
    radGrad.addColorStop(0, "#ffffff");
    radGrad.addColorStop(0.25, star.c);
    radGrad.addColorStop(0.6, "rgba(255, 213, 79, 0.35)");
    radGrad.addColorStop(1, "rgba(0, 0, 0, 0)");
    ctx.fillStyle = radGrad;
    ctx.beginPath();
    ctx.arc(star.x, star.y, star.r * 1.9, 0, Math.PI * 2);
    ctx.fill();

    const haloRings = [star.r * 1.35, star.r * 2.1, star.r * 2.95];
    const haloColors = ["#ffffff", star.c, "#e1f5fe"];

    haloRings.forEach((radius, ringIndex) => {
      const numDashes = Math.floor(radius * 1.35);
      const dashAngle = (Math.PI * 2) / numDashes;
      const col = haloColors[ringIndex % haloColors.length];
      const strokeW = 4.2 - ringIndex * 0.9;

      for (let i = 0; i < numDashes; i++) {
        const startA = i * dashAngle + ringIndex * 0.3;
        const endA = startA + dashAngle * 0.45;
        const x1 = star.x + Math.cos(startA) * radius;
        const y1 = star.y + Math.sin(startA) * radius;
        const x2 = star.x + Math.cos(endA) * radius;
        const y2 = star.y + Math.sin(endA) * radius;
        drawFlowStroke(x1, y1, col, strokeW, 14, 0.88);
      }
    });
  });

  // 4. Beautiful Textured Golden Crescent Moon in the Top-Right
  const moonX = 1880;
  const moonY = 170;
  const moonR = 66;

  const moonHalos = [moonR * 1.15, moonR * 1.65, moonR * 2.25, moonR * 2.85];
  moonHalos.forEach((radius, ringIdx) => {
    const numDashes = Math.floor(radius * 1.45);
    const dashAngle = (Math.PI * 2) / numDashes;
    const col = ringIdx % 2 === 0 ? "#ffffff" : "#ffd54f";
    const strokeW = 4.8 - ringIdx * 0.8;

    for (let i = 0; i < numDashes; i++) {
      const startA = i * dashAngle;
      const endA = startA + dashAngle * 0.55;
      const x1 = moonX + Math.cos(startA) * radius;
      const y1 = moonY + Math.sin(startA) * radius;
      const x2 = moonX + Math.cos(endA) * radius;
      const y2 = moonY + Math.sin(endA) * radius;
      drawFlowStroke(x1, y1, col, strokeW, 16, 0.92);
    }
  });

  const moonGrad = ctx.createRadialGradient(
    moonX,
    moonY,
    1,
    moonX,
    moonY,
    moonR,
  );
  moonGrad.addColorStop(0, "#ffffff");
  moonGrad.addColorStop(0.35, "#ffe082");
  moonGrad.addColorStop(0.65, "#ffb300");
  moonGrad.addColorStop(1, "#e67e22");

  ctx.fillStyle = moonGrad;
  ctx.shadowColor = "#ffb300";
  ctx.shadowBlur = 45;
  ctx.beginPath();
  ctx.arc(moonX, moonY, moonR, -Math.PI * 0.42, Math.PI * 0.76, false);
  ctx.arc(
    moonX - 24,
    moonY + 6,
    moonR - 8,
    Math.PI * 0.7,
    -Math.PI * 0.39,
    true,
  );
  ctx.closePath();
  ctx.fill();
  ctx.shadowBlur = 0;

  // 5. Flowing Rolling Hills at the Bottom
  const hillPoints = [];
  const numHillPoints = 120;
  for (let i = 0; i <= numHillPoints; i++) {
    const x = (i / numHillPoints) * logicalWidth;
    const y =
      830 +
      Math.sin((x / logicalWidth) * Math.PI * 2.8) * 38 +
      Math.cos((x / logicalWidth) * Math.PI * 5.6) * 14;
    hillPoints.push({ x, y });
  }

  const hillColors = ["#020814", "#081730", "#10264c", "#173466"];
  for (let layer = 0; layer < 4; layer++) {
    const layerOffset = layer * 32;
    const color = hillColors[layer];

    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.moveTo(0, logicalHeight);
    hillPoints.forEach((p, idx) => {
      const py = Math.min(logicalHeight, p.y + layerOffset - 18);
      ctx.lineTo(p.x, py);
    });
    ctx.lineTo(logicalWidth, logicalHeight);
    ctx.closePath();
    ctx.fill();

    for (let i = 0; i < hillPoints.length - 2; i += 2) {
      const p1 = hillPoints[i];
      const p2 = hillPoints[i + 2];
      drawFlowStroke(
        p1.x,
        p1.y + layerOffset - 16,
        "#245296",
        5.2 - layer,
        22,
        0.65,
      );
      drawFlowStroke(p1.x, p1.y + layerOffset - 12, "#081730", 4.2, 18, 0.45);
    }
  }

  // 5.5. Draw the iconic Starry Night Village silhouette in the valley
  function drawVillage() {
    ctx.save();
    const baseX = 1380;
    const baseY = 810;

    // Draw church spire (towering, stylized silhouette)
    ctx.fillStyle = "#01050c";
    ctx.beginPath();
    ctx.moveTo(baseX - 14, baseY + 40);
    ctx.lineTo(baseX - 14, baseY - 35);
    ctx.lineTo(baseX - 18, baseY - 35);
    ctx.lineTo(baseX, baseY - 95); // Spire tip pointing high
    ctx.lineTo(baseX + 18, baseY - 35);
    ctx.lineTo(baseX + 14, baseY - 35);
    ctx.lineTo(baseX + 14, baseY + 40);
    ctx.closePath();
    ctx.fill();

    // Church outline / body
    ctx.beginPath();
    ctx.moveTo(baseX - 38, baseY + 40);
    ctx.lineTo(baseX - 38, baseY - 5);
    ctx.lineTo(baseX - 14, baseY - 18);
    ctx.lineTo(baseX + 14, baseY - 18);
    ctx.lineTo(baseX + 38, baseY - 5);
    ctx.lineTo(baseX + 38, baseY + 40);
    ctx.closePath();
    ctx.fill();

    // Church glowing window
    ctx.fillStyle = "#ffe082";
    ctx.shadowColor = "#ffb300";
    ctx.shadowBlur = 8;
    ctx.fillRect(baseX - 5, baseY - 5, 10, 18);
    ctx.beginPath();
    ctx.arc(baseX, baseY - 5, 5, Math.PI, 0);
    ctx.fill();
    ctx.shadowBlur = 0;

    // Draw small houses surrounding the church
    const houseColors = ["#01050d", "#030a17", "#061022"];
    const numHouses = 16;
    for (let i = 0; i < numHouses; i++) {
      const hx = baseX + (i - numHouses / 2) * 38 + (Math.random() - 0.5) * 10;
      if (Math.abs(hx - baseX) < 22) continue; // Don't overlap church core

      const hy = baseY + 12 + Math.sin(hx * 0.05) * 10;
      const hw = 24 + Math.random() * 8;
      const hh = 15 + Math.random() * 6;

      // House body
      ctx.fillStyle = houseColors[i % houseColors.length];
      ctx.fillRect(hx - hw / 2, hy, hw, hh);

      // Gable roof
      ctx.beginPath();
      ctx.moveTo(hx - hw / 2 - 2, hy);
      ctx.lineTo(hx, hy - 9 - Math.random() * 3);
      ctx.lineTo(hx + hw / 2 + 2, hy);
      ctx.closePath();
      ctx.fill();

      // Tiny glowing windows (warm cadmium yellow/orange)
      if (Math.random() > 0.3) {
        ctx.fillStyle = "#ffd54f";
        ctx.shadowColor = "#ffca28";
        ctx.shadowBlur = 6;
        const wx = hx - 4 + Math.random() * 8;
        const wy = hy + 4 + Math.random() * 4;
        ctx.fillRect(wx - 2.5, wy - 2.5, 5, 5);
        ctx.shadowBlur = 0;
      }
    }
    ctx.restore();
  }

  // Draw the village in the valley
  drawVillage();

  // 6. Flaming Cypresses (Towering Dark Green/Bronze silhouetted branches on Left)
  const cypressBaseX = 260;
  const cypressBaseY = logicalHeight;
  const cypressW = 200;
  const cypressH = 760;

  function drawCypressBranch(cx, cy, cw, ch, isCore = false) {
    ctx.beginPath();
    ctx.moveTo(cx - cw / 2, cy);

    ctx.bezierCurveTo(
      cx - cw * 0.9,
      cy - ch * 0.35,
      cx - cw * 0.68,
      cy - ch * 0.78,
      cx - cw * 0.1,
      cy - ch,
    );
    ctx.lineTo(cx + cw * 0.05, cy - ch);
    ctx.bezierCurveTo(
      cx + cw * 0.48,
      cy - ch * 0.74,
      cx + cw * 0.68,
      cy - ch * 0.35,
      cx + cw / 2,
      cy,
    );
    ctx.closePath();

    if (isCore) {
      ctx.fillStyle = "#020503";
      ctx.fill();
    } else {
      const grad = ctx.createLinearGradient(cx, cy, cx, cy - ch);
      grad.addColorStop(0, "#050b06");
      grad.addColorStop(0.5, "#122616");
      grad.addColorStop(1, "#020503");
      ctx.fillStyle = grad;
      ctx.fill();
    }
  }

  drawCypressBranch(cypressBaseX, cypressBaseY, cypressW, cypressH, true);

  const cypressStrokeColors = [
    "#020603",
    "#061208",
    "#122a18",
    "#24452a",
    "#3c4011",
    "#0f2012",
  ];
  for (let i = 0; i < 220; i++) {
    const bh = cypressH * (0.16 + Math.random() * 0.8);
    const bw = cypressW * (0.22 + Math.random() * 0.58) * (1.0 - bh / cypressH);
    const bx =
      cypressBaseX +
      (Math.random() - 0.5) *
        (cypressW * 0.72) *
        (1.0 - (bh / cypressH) * 0.82);
    const by = cypressBaseY - Math.random() * (cypressH - bh * 1.15);

    const strokeColor =
      cypressStrokeColors[
        Math.floor(Math.random() * cypressStrokeColors.length)
      ];
    const strokeW = 4.2 + Math.random() * 5.8;

    const startX = bx - bw / 2;
    const startY = by;
    const ctrlX = bx + (Math.random() - 0.5) * bw;
    const ctrlY = by - bh * 0.42;
    const endX = bx + (Math.random() - 0.5) * (bw * 0.28);
    const endY = by - bh;

    ctx.save();
    ctx.lineCap = "round";
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = strokeW;
    ctx.beginPath();
    ctx.moveTo(startX, startY);
    ctx.quadraticCurveTo(ctrlX, ctrlY, endX, endY);
    ctx.stroke();
    ctx.restore();
  }

  // 7. Core Premium Touch: Multi-layered High-frequency Impasto Paint Ridge Overlay (optimized size & counts)
  ctx.save();
  ctx.globalCompositeOperation = "overlay";
  ctx.lineWidth = 1.0;
  for (let i = 0; i < 12000; i++) {
    const rx = Math.random() * logicalWidth;
    const ry = Math.random() * logicalHeight;
    const len = 1.5 + Math.random() * 2.0;
    const angle = Math.random() * Math.PI * 2;

    ctx.strokeStyle =
      Math.random() > 0.5 ? "rgba(255,255,255,0.065)" : "rgba(0,0,0,0.055)";
    ctx.beginPath();
    ctx.moveTo(rx, ry);
    ctx.lineTo(rx + Math.cos(angle) * len, ry + Math.sin(angle) * len);
    ctx.stroke();
  }
  ctx.restore();

  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.ClampToEdgeWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createGlowingStarTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 64;
  canvas.height = 64;
  const ctx = canvas.getContext("2d");

  const grad = ctx.createRadialGradient(32, 32, 0, 32, 32, 32);
  grad.addColorStop(0, "rgba(255, 255, 255, 1)");
  grad.addColorStop(0.12, "rgba(240, 245, 255, 0.95)");
  grad.addColorStop(0.28, "rgba(140, 200, 255, 0.65)");
  grad.addColorStop(0.55, "rgba(80, 120, 255, 0.2)");
  grad.addColorStop(1, "rgba(0, 0, 0, 0)");

  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, 64, 64);

  return new THREE.CanvasTexture(canvas);
}

function createNebulaTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  // Scale coordinate system to draw 2048x1024 coordinate system seamlessly at 1024x512
  ctx.scale(0.5, 0.5);
  const logicalWidth = 2048;
  const logicalHeight = 1024;

  const bgGrad = ctx.createLinearGradient(0, 0, 0, logicalHeight);
  bgGrad.addColorStop(0, "#010106");
  bgGrad.addColorStop(0.5, "#03030d");
  bgGrad.addColorStop(1, "#010104");
  ctx.fillStyle = bgGrad;
  ctx.fillRect(0, 0, logicalWidth, logicalHeight);

  function drawNebulaCloud(x, y, rx, ry, colorStop1, colorStop2, angle) {
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(angle);
    ctx.scale(rx / ry, 1);
    const grad = ctx.createRadialGradient(0, 0, 0, 0, 0, ry);
    grad.addColorStop(0, colorStop1);
    grad.addColorStop(0.5, colorStop2);
    grad.addColorStop(1, "rgba(0,0,0,0)");
    ctx.fillStyle = grad;
    ctx.beginPath();
    ctx.arc(0, 0, ry, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  drawNebulaCloud(
    1250,
    480,
    500,
    300,
    "rgba(110, 32, 192, 0.24)",
    "rgba(40, 20, 100, 0.08)",
    0.4,
  );
  drawNebulaCloud(
    1350,
    520,
    350,
    200,
    "rgba(220, 40, 150, 0.18)",
    "rgba(130, 20, 120, 0.04)",
    -0.2,
  );
  drawNebulaCloud(
    600,
    400,
    450,
    250,
    "rgba(20, 140, 220, 0.2)",
    "rgba(10, 40, 120, 0.06)",
    -0.3,
  );
  drawNebulaCloud(
    700,
    380,
    250,
    150,
    "rgba(40, 210, 255, 0.14)",
    "rgba(20, 80, 160, 0.04)",
    0.25,
  );
  drawNebulaCloud(
    1600,
    600,
    400,
    180,
    "rgba(235, 130, 30, 0.15)",
    "rgba(120, 50, 10, 0.04)",
    0.6,
  );
  drawNebulaCloud(
    950,
    300,
    300,
    150,
    "rgba(255, 180, 50, 0.08)",
    "rgba(180, 80, 20, 0.02)",
    -0.5,
  );

  for (let i = 0; i < 30; i++) {
    const cx = Math.random() * logicalWidth;
    const cy = Math.random() * logicalHeight;
    const cr = 40 + Math.random() * 120;
    const op = 0.03 + Math.random() * 0.06;
    const col =
      Math.random() > 0.5
        ? `rgba(130, 50, 200, ${op})`
        : `rgba(30, 120, 200, ${op})`;
    drawNebulaCloud(
      cx,
      cy,
      cr,
      cr * 0.6,
      col,
      "rgba(0,0,0,0)",
      Math.random() * Math.PI,
    );
  }

  return new THREE.CanvasTexture(canvas);
}

function createGasGiantTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  const bands = [
    { pos: 0, col: "#2a1508" },
    { pos: 0.15, col: "#5e3e29" },
    { pos: 0.28, col: "#a27a5c" },
    { pos: 0.38, col: "#d4b395" },
    { pos: 0.48, col: "#cca07a" },
    { pos: 0.58, col: "#946142" },
    { pos: 0.7, col: "#5e3e29" },
    { pos: 0.85, col: "#d4b395" },
    { pos: 1.0, col: "#2a1508" },
  ];

  const grad = ctx.createLinearGradient(0, 0, 0, canvas.height);
  bands.forEach((b) => grad.addColorStop(b.pos, b.col));
  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.globalAlpha = 0.12;
  for (let i = 0; i < 40; i++) {
    const y = Math.random() * canvas.height;
    const h = 2 + Math.random() * 8;
    ctx.fillStyle = Math.random() > 0.5 ? "#ffffff" : "#000000";
    ctx.fillRect(0, y, canvas.width, h);
  }
  ctx.globalAlpha = 1.0;

  return new THREE.CanvasTexture(canvas);
}

function createRingTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 256;
  canvas.height = 8;
  const ctx = canvas.getContext("2d");

  const grad = ctx.createLinearGradient(0, 0, canvas.width, 0);
  grad.addColorStop(0, "rgba(212, 175, 55, 0)");
  grad.addColorStop(0.1, "rgba(212, 175, 55, 0.15)");
  grad.addColorStop(0.3, "rgba(230, 210, 160, 0.45)");
  grad.addColorStop(0.4, "rgba(150, 120, 80, 0.1)");
  grad.addColorStop(0.6, "rgba(240, 220, 180, 0.55)");
  grad.addColorStop(0.8, "rgba(212, 175, 55, 0.25)");
  grad.addColorStop(0.95, "rgba(212, 175, 55, 0.05)");
  grad.addColorStop(1, "rgba(0, 0, 0, 0)");

  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  return new THREE.CanvasTexture(canvas);
}

function createVolcanic2DTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  // Dark basalt base
  ctx.fillStyle = "#0c0a0a";
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  // Basalt cracked landmass patterns
  ctx.fillStyle = "#1e1614";
  for (let i = 0; i < 60; i++) {
    const x = Math.random() * canvas.width;
    const y = Math.random() * canvas.height;
    const r = 20 + Math.random() * 45;
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    ctx.fill();
  }

  // Glowing active lava networks
  ctx.lineWidth = 2.2;
  ctx.shadowBlur = 12;

  const lavaColors = ["#ff3300", "#ff6600", "#ffaa00", "#ffdd00"];
  for (let i = 0; i < 28; i++) {
    ctx.strokeStyle = lavaColors[Math.floor(Math.random() * lavaColors.length)];
    ctx.shadowColor = ctx.strokeStyle;
    ctx.beginPath();
    let cx = Math.random() * canvas.width;
    let cy = Math.random() * canvas.height;
    ctx.moveTo(cx, cy);

    const steps = 8 + Math.floor(Math.random() * 12);
    for (let j = 0; j < steps; j++) {
      cx += (Math.random() - 0.5) * 45;
      cy += (Math.random() - 0.5) * 22;
      ctx.lineTo(cx, cy);
    }
    ctx.stroke();
  }
  ctx.shadowBlur = 0;

  return new THREE.CanvasTexture(canvas);
}

function createIceGiant2DTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  // Deep serene teal-blue base
  const grad = ctx.createLinearGradient(0, 0, 0, canvas.height);
  grad.addColorStop(0, "#002b47");
  grad.addColorStop(0.5, "#0b4d75");
  grad.addColorStop(1, "#001a2e");
  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  // Wispy high-altitude ice clouds (soft white-turquoise bands)
  ctx.globalAlpha = 0.18;
  for (let i = 0; i < 18; i++) {
    const y = Math.random() * canvas.height;
    const h = 4 + Math.random() * 18;
    const cloudGrad = ctx.createLinearGradient(0, y, 0, y + h);
    cloudGrad.addColorStop(0, "rgba(140, 230, 255, 0)");
    cloudGrad.addColorStop(0.5, "rgba(200, 245, 255, 0.85)");
    cloudGrad.addColorStop(1, "rgba(140, 230, 255, 0)");
    ctx.fillStyle = cloudGrad;

    ctx.beginPath();
    ctx.moveTo(0, y);
    for (let x = 0; x <= canvas.width; x += 30) {
      const cy = y + Math.sin((x / canvas.width) * Math.PI * 4) * 8;
      ctx.lineTo(x, cy + h / 2);
    }
    ctx.lineTo(canvas.width, y + h);
    ctx.lineTo(0, y + h);
    ctx.closePath();
    ctx.fill();
  }
  ctx.globalAlpha = 1.0;

  return new THREE.CanvasTexture(canvas);
}

function createSilverMoonTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  // Silver-white to Slate gradient base
  const baseGrad = ctx.createLinearGradient(0, 0, 0, canvas.height);
  baseGrad.addColorStop(0, "#d5dce6");
  baseGrad.addColorStop(0.5, "#a6b0c2");
  baseGrad.addColorStop(1, "#7b8599");
  ctx.fillStyle = baseGrad;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  // Darker highlands and basalt maria patches (smooth circular splotches)
  ctx.fillStyle = "#5c657a";
  ctx.globalAlpha = 0.45;
  for (let i = 0; i < 15; i++) {
    const x = Math.random() * canvas.width;
    const y = Math.random() * canvas.height;
    const r = 25 + Math.random() * 50;
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.globalAlpha = 1.0;

  // Render crater rims and radiating impact rays
  ctx.strokeStyle = "#e8edf5";
  ctx.lineWidth = 1.5;
  ctx.fillStyle = "#8a94ab";

  for (let i = 0; i < 22; i++) {
    const cx = Math.random() * canvas.width;
    const cy = Math.random() * canvas.height;
    const cr = 6 + Math.random() * 18;

    // Crater bowl
    ctx.globalAlpha = 0.35;
    ctx.beginPath();
    ctx.arc(cx, cy, cr, 0, Math.PI * 2);
    ctx.fill();

    // Crater rim highlight
    ctx.globalAlpha = 0.8;
    ctx.beginPath();
    ctx.arc(cx, cy, cr, Math.PI * 0.75, Math.PI * 1.75); // Sunward highlight
    ctx.stroke();

    // Crater rim shadow
    ctx.strokeStyle = "#3b4152";
    ctx.beginPath();
    ctx.arc(cx, cy, cr, -Math.PI * 0.25, Math.PI * 0.75); // Dark side shadow
    ctx.stroke();
    ctx.strokeStyle = "#e8edf5"; // restore light color

    // Radiating rays for large craters
    if (cr > 12) {
      ctx.globalAlpha = 0.22;
      ctx.lineWidth = 0.8;
      const numRays = 6 + Math.floor(Math.random() * 6);
      for (let rIdx = 0; rIdx < numRays; rIdx++) {
        const angle = (rIdx * Math.PI * 2) / numRays + Math.random() * 0.2;
        const rayLen = cr * (2.2 + Math.random() * 3.5);
        ctx.beginPath();
        ctx.moveTo(cx + Math.cos(angle) * cr, cy + Math.sin(angle) * cr);
        ctx.lineTo(
          cx + Math.cos(angle) * rayLen,
          cy + Math.sin(angle) * rayLen,
        );
        ctx.stroke();
      }
      ctx.lineWidth = 1.5;
    }
  }
  ctx.globalAlpha = 1.0;

  return new THREE.CanvasTexture(canvas);
}

function createCosmicSwirlTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  // Scale coordinate system so 1024x1024 logic draws seamlessly at 512x512
  ctx.scale(0.5, 0.5);
  const logicalWidth = 1024;
  const logicalHeight = 1024;

  // Clear/transparent base
  ctx.fillStyle = "rgba(0,0,0,0)";
  ctx.fillRect(0, 0, logicalWidth, logicalHeight);

  // Draw a gorgeous swirling spiral galaxy
  const centerX = 512;
  const centerY = 512;
  const numArms = 3;

  ctx.save();
  // Radial gradient in center for the bright galactic nucleus
  const centerGrad = ctx.createRadialGradient(
    centerX,
    centerY,
    0,
    centerX,
    centerY,
    80,
  );
  centerGrad.addColorStop(0, "rgba(255, 235, 150, 0.95)"); // Bright golden core
  centerGrad.addColorStop(0.3, "rgba(255, 170, 80, 0.75)");
  centerGrad.addColorStop(0.6, "rgba(180, 80, 255, 0.4)");
  centerGrad.addColorStop(1, "rgba(0, 0, 0, 0)");
  ctx.fillStyle = centerGrad;
  ctx.beginPath();
  ctx.arc(centerX, centerY, 80, 0, Math.PI * 2);
  ctx.fill();
  ctx.restore();

  // Draw arms using many glowing particles
  const armColors = [
    "rgba(255, 202, 40, 0.28)", // Gold
    "rgba(3, 169, 244, 0.25)", // Cyan
    "rgba(156, 39, 176, 0.22)", // Violet
    "rgba(255, 255, 255, 0.45)", // White hot sparks
  ];

  for (let arm = 0; arm < numArms; arm++) {
    const armAngleOffset = (arm * Math.PI * 2) / numArms;
    for (let i = 0; i < 700; i++) {
      // Optimized from 2200 to 700
      const r = 20 + Math.pow(Math.random(), 1.4) * 440;
      // Spiral formula: angle = base_offset + log/linear spiral
      const theta = armAngleOffset + r * 0.012 + (Math.random() - 0.5) * 0.42;
      const px = centerX + Math.cos(theta) * r;
      const py = centerY + Math.sin(theta) * r;

      // Color selection based on distance
      let color = armColors[0];
      if (r > 280) {
        color = armColors[2]; // Outer arms violet
      } else if (r > 130) {
        color = armColors[1]; // Mid arms cyan
      }
      if (Math.random() > 0.95) color = armColors[3]; // Hot stars

      const pSize = 1.2 + Math.random() * 4.5 * (1.0 - r / 500);

      ctx.fillStyle = color;
      ctx.beginPath();
      ctx.arc(px, py, pSize, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  return new THREE.CanvasTexture(canvas);
}

function createCosmicSwirlNebula() {
  const texture = createCosmicSwirlTexture();
  const geometry = new THREE.PlaneGeometry(28, 28);
  const material = new THREE.MeshBasicMaterial({
    map: texture,
    transparent: true,
    opacity: 0.85,
    blending: THREE.AdditiveBlending,
    side: THREE.DoubleSide,
    depthWrite: false,
  });
  const cosmicSwirl = new THREE.Mesh(geometry, material);
  // Position deep in the background for amazing parallax depth
  cosmicSwirl.position.set(-15, 8, -25);
  cosmicSwirl.rotation.x = Math.PI / 12; // tilt towards viewer
  cosmicSwirl.name = "cosmos-swirl";
  state.scene.add(cosmicSwirl);
  state.cosmos.cosmicSwirl = cosmicSwirl;
}

function createStardustParticles() {
  const count = 120;
  const geometry = new THREE.BufferGeometry();
  const positions = new Float32Array(count * 3);
  const colors = new Float32Array(count * 3);
  const sizes = new Float32Array(count);

  const stardustSpeeds = [];

  const gold = new THREE.Color(0xffca28);
  const cyan = new THREE.Color(0x80deea);
  const violet = new THREE.Color(0xb39ddb);
  const white = new THREE.Color(0xffffff);

  for (let i = 0; i < count; i++) {
    // Space is centered around E/W and height.
    // From Z = -15 (outside window) to Z = 4 (inside the room)
    positions[i * 3] = -5.0 + Math.random() * 9.0; // X span
    positions[i * 3 + 1] = 0.5 + Math.random() * 5.0; // Y span
    positions[i * 3 + 2] = -15 + Math.random() * 19; // Z span

    // Palette of gold, sky blue, magic violet, white
    const r = Math.random();
    const col = r > 0.75 ? gold : r > 0.5 ? cyan : r > 0.25 ? violet : white;
    colors[i * 3] = col.r;
    colors[i * 3 + 1] = col.g;
    colors[i * 3 + 2] = col.b;

    sizes[i] = 0.04 + Math.random() * 0.08;

    // Speeds and sine wave offsets for swaying motion
    stardustSpeeds.push({
      speedZ: 0.01 + Math.random() * 0.016,
      swaySpeedX: 0.25 + Math.random() * 0.45,
      swaySpeedY: 0.25 + Math.random() * 0.45,
      swayAmpX: 0.08 + Math.random() * 0.12,
      swayAmpY: 0.08 + Math.random() * 0.12,
      offsetX: Math.random() * Math.PI * 2,
      offsetY: Math.random() * Math.PI * 2,
    });
  }

  geometry.setAttribute("position", new THREE.BufferAttribute(positions, 3));
  geometry.setAttribute("color", new THREE.BufferAttribute(colors, 3));
  geometry.setAttribute("size", new THREE.BufferAttribute(sizes, 1));

  const starTexture = createGlowingStarTexture();
  const material = new THREE.PointsMaterial({
    size: 0.12,
    sizeAttenuation: true,
    vertexColors: true,
    transparent: true,
    opacity: 0.9,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
    map: starTexture,
  });

  const points = new THREE.Points(geometry, material);
  points.name = "cosmos-stardust";
  state.scene.add(points);
  state.cosmos.stardust = points;
  state.cosmos.stardustSpeeds = stardustSpeeds;
}

function createAtmosphericGlow(size, colorHex) {
  const geo = new THREE.SphereGeometry(size * 1.02, 64, 64);
  const color = new THREE.Color(colorHex);
  const mat = new THREE.ShaderMaterial({
    vertexShader: `
            varying vec3 vNormal;
            void main() {
                vNormal = normalize(normalMatrix * normal);
                gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
            }
        `,
    fragmentShader: `
            varying vec3 vNormal;
            uniform vec3 glowColor;
            void main() {
                // Rayleigh scattering edge-glow: bright at normal perpendiculars to view
                float intensity = pow(0.68 - dot(vNormal, vec3(0.0, 0.0, 1.0)), 4.0);
                gl_FragColor = vec4(glowColor, 1.0) * intensity;
            }
        `,
    uniforms: {
      glowColor: { value: color },
    },
    blending: THREE.AdditiveBlending,
    side: THREE.BackSide,
    transparent: true,
    depthWrite: false,
  });
  return new THREE.Mesh(geo, mat);
}

/* ─── 3D Mesh Assemblies ─── */

function createCosmosNebulaSkybox() {
  const texture = createNebulaTexture();
  const geometry = new THREE.SphereGeometry(32, 32, 24);
  const material = new THREE.MeshBasicMaterial({
    map: texture,
    side: THREE.BackSide,
    depthWrite: false,
  });
  const skybox = new THREE.Mesh(geometry, material);
  skybox.name = "cosmos-skybox";
  state.scene.add(skybox);
  state.cosmos.skybox = skybox;
}

function createBackgroundPlanet() {
  const planetGroup = new THREE.Group();
  planetGroup.name = "cosmos-planet-group";

  // ─── Planet 1: Aurelia (Banded Saturn-like Gas Giant) ───
  const aureliaGroup = new THREE.Group();
  aureliaGroup.name = "aurelia-group";

  const planetTexture = createGasGiantTexture();
  const planetGeo = new THREE.SphereGeometry(1.6, 32, 32);
  const planetMat = new THREE.MeshStandardMaterial({
    map: planetTexture,
    roughness: 0.8,
    metalness: 0.1,
  });
  const planet = new THREE.Mesh(planetGeo, planetMat);
  planet.castShadow = true;
  planet.receiveShadow = true;
  aureliaGroup.add(planet);

  // Aurelia Atmospheric Glow Haze
  const aureliaGlow = createAtmosphericGlow(1.6, 0xffd2a0);
  aureliaGroup.add(aureliaGlow);
  state.cosmos.aureliaGlow = aureliaGlow; // Bind for pulsing

  // Aurelia Rings
  const ringTexture = createRingTexture();
  const ringGeo = new THREE.RingGeometry(2.0, 3.8, 64);

  // Map ring texture U/V coordinate concentrically
  const pos = ringGeo.attributes.position;
  const uvs = ringGeo.attributes.uv;
  for (let i = 0; i < pos.count; i++) {
    const x = pos.getX(i);
    const y = pos.getY(i);
    const r = Math.sqrt(x * x + y * y);
    const u = (r - 2.0) / (3.8 - 2.0);
    uvs.setXY(i, u, 0.5);
  }
  uvs.needsUpdate = true;

  const ringMat = new THREE.MeshStandardMaterial({
    map: ringTexture,
    side: THREE.DoubleSide,
    transparent: true,
    opacity: 0.82,
    roughness: 0.6,
    metalness: 0.1,
    depthWrite: false,
  });
  const rings = new THREE.Mesh(ringGeo, ringMat);
  rings.rotation.x = Math.PI / 2.2;
  rings.rotation.y = 0.2;
  aureliaGroup.add(rings);

  // Position Aurelia in primary window sight line
  aureliaGroup.position.set(0, 0, 0); // local center
  aureliaGroup.rotation.z = 0.45; // Tilt axis
  planetGroup.add(aureliaGroup);
  state.cosmos.aureliaMesh = planet; // Bind for spin

  // ─── Planet 4: Selene (Mystical Silver Moon) ───
  const seleneGroup = new THREE.Group();
  seleneGroup.name = "selene-group";

  const seleneTexture = createSilverMoonTexture();
  const seleneGeo = new THREE.SphereGeometry(0.7, 32, 32);
  const seleneMat = new THREE.MeshStandardMaterial({
    map: seleneTexture,
    roughness: 0.85,
    metalness: 0.05,
  });
  const selene = new THREE.Mesh(seleneGeo, seleneMat);
  selene.castShadow = true;
  selene.receiveShadow = true;
  seleneGroup.add(selene);

  // Selene Silver Atmospheric Glow
  const seleneGlow = createAtmosphericGlow(0.7, 0xd0e0f0);
  seleneGroup.add(seleneGlow);
  state.cosmos.seleneGlow = seleneGlow;

  // Position Selene in the upper right background
  seleneGroup.position.set(5.5, 6.0, -8.0);
  seleneGroup.rotation.z = 0.28;
  planetGroup.add(seleneGroup);
  state.cosmos.seleneMesh = selene;

  // ─── Set Group Base Position in space (visible through window) ───
  planetGroup.position.set(-8.5, 4.2, -17.5);
  state.scene.add(planetGroup);
  state.cosmos.planetGroup = planetGroup;

  // Direct lighting specifically for the planet's dramatic crescent shade
  const planetLight = new THREE.DirectionalLight(0xfff5ea, 2.5);
  planetLight.position.set(15, 10, -5);
  state.scene.add(planetLight);
  state.cosmos.planetLight = planetLight;
}

function createCosmosStarfields() {
  const starTexture = createGlowingStarTexture();

  // Layer 1: Background Tiny Stars (dense, deep)
  const count1 = 1800;
  const geo1 = new THREE.BufferGeometry();
  const pos1 = new Float32Array(count1 * 3);
  const col1 = new Float32Array(count1 * 3);
  const size1 = new Float32Array(count1);

  for (let i = 0; i < count1; i++) {
    const radius = 26 + Math.random() * 5;
    const theta = Math.random() * Math.PI * 2;
    const phi = Math.acos(Math.random() * 2 - 1);

    pos1[i * 3] = Math.sin(phi) * Math.cos(theta) * radius;
    pos1[i * 3 + 1] = Math.sin(phi) * Math.sin(theta) * radius;
    pos1[i * 3 + 2] = Math.cos(phi) * radius;

    col1[i * 3] = 0.95 + Math.random() * 0.05;
    col1[i * 3 + 1] = 0.95 + Math.random() * 0.05;
    col1[i * 3 + 2] = 1.0;

    size1[i] = 0.02 + Math.random() * 0.03;
  }

  geo1.setAttribute("position", new THREE.BufferAttribute(pos1, 3));
  geo1.setAttribute("color", new THREE.BufferAttribute(col1, 3));
  geo1.setAttribute("size", new THREE.BufferAttribute(size1, 1));

  const starfield1 = new THREE.Points(
    geo1,
    new THREE.PointsMaterial({
      size: 0.06,
      sizeAttenuation: true,
      vertexColors: true,
      transparent: true,
      opacity: 0.75,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
      map: starTexture,
    }),
  );
  starfield1.name = "cosmos-starfield-bg";
  state.scene.add(starfield1);
  state.cosmos.starfield1 = starfield1;

  // Layer 2: Foreground Clusters (bright, colorful, twinkling)
  const count2 = 250;
  const geo2 = new THREE.BufferGeometry();
  const pos2 = new Float32Array(count2 * 3);
  const col2 = new Float32Array(count2 * 3);
  const size2 = new Float32Array(count2);

  const gold = new THREE.Color(0xffd54f);
  const blue = new THREE.Color(0x8ec8ff);
  const magenta = new THREE.Color(0xf8bbd0);
  const white = new THREE.Color(0xffffff);

  for (let i = 0; i < count2; i++) {
    const radius = 15 + Math.random() * 10;
    const theta = Math.random() * Math.PI * 2;
    const phi = Math.acos(Math.random() * 2 - 1);

    pos2[i * 3] = Math.sin(phi) * Math.cos(theta) * radius;
    pos2[i * 3 + 1] = Math.sin(phi) * Math.sin(theta) * radius;
    pos2[i * 3 + 2] = Math.cos(phi) * radius;

    const baseCol =
      i % 8 === 0 ? gold : i % 5 === 0 ? magenta : i % 3 === 0 ? blue : white;
    col2[i * 3] = baseCol.r;
    col2[i * 3 + 1] = baseCol.g;
    col2[i * 3 + 2] = baseCol.b;

    size2[i] = 0.06 + Math.random() * 0.08;
  }

  geo2.setAttribute("position", new THREE.BufferAttribute(pos2, 3));
  geo2.setAttribute("color", new THREE.BufferAttribute(col2, 3));
  geo2.setAttribute("size", new THREE.BufferAttribute(size2, 1));

  const starfield2 = new THREE.Points(
    geo2,
    new THREE.PointsMaterial({
      size: 0.12,
      sizeAttenuation: true,
      vertexColors: true,
      transparent: true,
      opacity: 0.95,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
      map: starTexture,
    }),
  );
  starfield2.name = "cosmos-starfield-fg";
  state.scene.add(starfield2);
  state.cosmos.starfield2 = starfield2;
}

function createShootingStars() {
  const group = new THREE.Group();
  group.name = "cosmos-shooting-stars";

  // Smooth trailing alpha fade cylinder
  const trailGeo = new THREE.CylinderGeometry(0.002, 0.015, 1.2, 6, 1, true);

  // Shift geometry center so scaling matches trailing look
  trailGeo.translate(0, 0.6, 0);

  const trailMat = new THREE.MeshBasicMaterial({
    color: 0xe0f2fe,
    transparent: true,
    opacity: 0.68,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
    side: THREE.DoubleSide,
  });

  for (let i = 0; i < 8; i++) {
    const star = new THREE.Mesh(trailGeo, trailMat);
    // Distribute in space visible outside window
    star.position.set(
      -12 + Math.random() * 6,
      5 + Math.random() * 6,
      -18 + Math.random() * 8,
    );
    star.rotation.set(Math.PI / 2.3, 0.1, -0.75 + Math.random() * 0.2);
    star.scale.set(1.0, 0.5 + Math.random() * 1.0, 1.0);
    star.userData.speed = 0.005 + Math.random() * 0.01;
    star.userData.baseY = star.position.y;
    star.userData.baseX = star.position.x;
    group.add(star);
  }

  state.scene.add(group);
  state.cosmos.shootingStars = group;
}

function createTelescope() {
  const telescope = new THREE.Group();
  telescope.name = "cosmos-telescope";

  // ─── Materials ───
  const mahoganyMat = new THREE.MeshStandardMaterial({
    color: 0x5a2d13, // Rich Mahogany dark brown wood
    roughness: 0.15,
    metalness: 0.02,
  });
  const brassMat = new THREE.MeshStandardMaterial({
    color: 0xd4af37, // Polished Gold Brass
    roughness: 0.18,
    metalness: 0.95,
  });
  const navyMat = new THREE.MeshStandardMaterial({
    color: 0x0a1b2d, // Sleek sapphire blue metallic OTA barrel
    roughness: 0.2,
    metalness: 0.85,
  });
  const steelMat = new THREE.MeshStandardMaterial({
    color: 0x7a7a7a,
    roughness: 0.25,
    metalness: 0.8,
  });
  const lensMat = new THREE.MeshStandardMaterial({
    color: 0xd0f5ff,
    emissive: 0x2299ff,
    emissiveIntensity: 1.5,
    roughness: 0.08,
    transparent: true,
    opacity: 0.85,
  });

  // ─── Static Tripod Legs & Mount ───

  // Circular floor brass plate
  const basePlate = new THREE.Mesh(
    new THREE.CylinderGeometry(0.24, 0.25, 0.015, 12),
    brassMat,
  );
  basePlate.position.y = 0.008;
  telescope.add(basePlate);

  // Tripod Legs Assembly (Tapered wood with brass accents)
  const legLength = 1.15;
  const legGeo = new THREE.CylinderGeometry(0.014, 0.008, legLength, 8);
  // Offset center of cylinder to swing legs outwards easily
  legGeo.translate(0, -legLength / 2, 0);

  const angles = [0, (Math.PI * 2) / 3, (Math.PI * 4) / 3];
  angles.forEach((angle) => {
    const legGroup = new THREE.Group();
    legGroup.position.set(0, 1.25, 0); // hinge height

    // Mahogany wooden shaft
    const shaft = new THREE.Mesh(legGeo, mahoganyMat);
    legGroup.add(shaft);

    // Brass collar locks at joints
    const collar1 = new THREE.Mesh(
      new THREE.CylinderGeometry(0.018, 0.018, 0.04, 8),
      brassMat,
    );
    collar1.position.y = -0.3;
    shaft.add(collar1);

    const collar2 = new THREE.Mesh(
      new THREE.CylinderGeometry(0.015, 0.015, 0.04, 8),
      brassMat,
    );
    collar2.position.y = -0.75;
    shaft.add(collar2);

    // Golden pointed metal feet
    const foot = new THREE.Mesh(
      new THREE.CylinderGeometry(0.008, 0.001, 0.08, 8),
      brassMat,
    );
    foot.position.y = -legLength - 0.04;
    shaft.add(foot);

    // Angle the legs outward
    legGroup.rotation.y = angle;
    legGroup.rotation.z = 0.18; // tilt outward

    telescope.add(legGroup);
  });

  // Central brass mounting vertical column
  const column = new THREE.Mesh(
    new THREE.CylinderGeometry(0.024, 0.024, 0.22, 12),
    brassMat,
  );
  column.position.set(0, 1.34, 0);
  column.castShadow = true;
  telescope.add(column);

  // Mount dial gears
  const dial = new THREE.Mesh(
    new THREE.CylinderGeometry(0.045, 0.045, 0.024, 16),
    brassMat,
  );
  dial.position.set(0, 1.45, 0);
  telescope.add(dial);

  // Equatorial Axis Block (tilt hinge)
  const hinge = new THREE.Mesh(
    new THREE.BoxGeometry(0.06, 0.08, 0.07),
    brassMat,
  );
  hinge.position.set(0, 1.49, 0);
  hinge.rotation.x = 0.4; // equatorial offset
  telescope.add(hinge);

  // Counterweight shaft and brass weight
  const weightShaft = new THREE.Mesh(
    new THREE.CylinderGeometry(0.006, 0.006, 0.35, 8),
    steelMat,
  );
  weightShaft.position.set(0.08, 1.44, 0.02);
  weightShaft.rotation.z = -0.8;
  telescope.add(weightShaft);

  const counterWeight = new THREE.Mesh(
    new THREE.CylinderGeometry(0.038, 0.038, 0.06, 12),
    brassMat,
  );
  counterWeight.position.set(0.18, 1.34, 0.02);
  counterWeight.rotation.z = -0.8;
  telescope.add(counterWeight);

  // ─── Tube Assembly Subgroup (Aligned along local Z axis) ───
  const tubeGroup = new THREE.Group();
  tubeGroup.position.set(0, 1.54, 0); // place tube center at top of mount hinge

  // Main Tube (OTA barrel - Sapphire Navy Metallic)
  const otaLength = 0.88;
  const mainTube = new THREE.Mesh(
    new THREE.CylinderGeometry(0.038, 0.045, otaLength, 18),
    navyMat,
  );
  mainTube.rotation.x = Math.PI / 2; // align along local Z axis
  mainTube.castShadow = true;
  tubeGroup.add(mainTube);

  // Brass Dew Shield (Front collar - larger cylinder)
  const shieldLength = 0.16;
  const dewShield = new THREE.Mesh(
    new THREE.CylinderGeometry(0.052, 0.052, shieldLength, 18),
    brassMat,
  );
  dewShield.position.set(0, 0, otaLength / 2 + shieldLength / 2 - 0.04);
  dewShield.rotation.x = Math.PI / 2;
  tubeGroup.add(dewShield);

  // Holographic Objective Lens
  const lens = new THREE.Mesh(
    new THREE.CylinderGeometry(0.048, 0.048, 0.015, 24),
    lensMat,
  );
  lens.position.set(0, 0, otaLength / 2 + shieldLength - 0.045);
  lens.rotation.x = Math.PI / 2;
  tubeGroup.add(lens);

  // Small glowing cyan light source inside tube shining out
  const lensLight = new THREE.PointLight(0x00d2ff, 1.6, 2.5, 2);
  lensLight.position.set(0, 0, otaLength / 2 + shieldLength - 0.03);
  tubeGroup.add(lensLight);
  state.cosmos.ambientStarLight2 = lensLight; // bind for pulsation

  // Volumetric active light beam casting out of window into space
  const beamLength = 3.6;
  const beamGeo = new THREE.ConeGeometry(0.68, beamLength, 20, 1, true);
  // Align base of cone at the lens center, extend along +Z
  beamGeo.translate(0, beamLength / 2, 0);
  const beamMat = new THREE.MeshBasicMaterial({
    color: 0x6ea8ff,
    transparent: true,
    opacity: 0.08,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
    side: THREE.DoubleSide,
  });
  const volumetricBeam = new THREE.Mesh(beamGeo, beamMat);
  volumetricBeam.raycast = () => {}; // Prevent this huge volumetric cone from blocking window clicks
  volumetricBeam.position.set(0, 0, otaLength / 2 + shieldLength - 0.045);
  volumetricBeam.rotation.x = Math.PI / 2; // point along Z axis
  tubeGroup.add(volumetricBeam);

  // Stack of brass collars on barrel
  [-0.2, 0, 0.2].forEach((offset) => {
    const ring = new THREE.Mesh(
      new THREE.TorusGeometry(0.046, 0.005, 8, 24),
      brassMat,
    );
    ring.position.set(0, 0, offset);
    tubeGroup.add(ring);
  });

  // Eyepiece Assembly (Back collar)
  const focuser = new THREE.Mesh(
    new THREE.CylinderGeometry(0.024, 0.024, 0.12, 12),
    brassMat,
  );
  focuser.position.set(0, 0, -otaLength / 2 - 0.05);
  focuser.rotation.x = Math.PI / 2;
  tubeGroup.add(focuser);

  // Angled brass mirror housing block
  const mirrorBox = new THREE.Mesh(
    new THREE.BoxGeometry(0.045, 0.045, 0.045),
    brassMat,
  );
  mirrorBox.position.set(0, 0, -otaLength / 2 - 0.12);
  tubeGroup.add(mirrorBox);

  const eyepiece = new THREE.Mesh(
    new THREE.CylinderGeometry(0.016, 0.016, 0.08, 12),
    brassMat,
  );
  eyepiece.position.set(0, 0.05, -otaLength / 2 - 0.14);
  eyepiece.rotation.x = Math.PI / 5; // angled up for easy viewing
  tubeGroup.add(eyepiece);

  const rubberCap = new THREE.Mesh(
    new THREE.CylinderGeometry(0.02, 0.02, 0.01, 12),
    new THREE.MeshStandardMaterial({ color: 0x111111, roughness: 0.8 }),
  );
  rubberCap.position.set(0, 0.085, -otaLength / 2 - 0.17);
  rubberCap.rotation.x = Math.PI / 5;
  tubeGroup.add(rubberCap);

  // Miniature Finder Scope mounted on top
  const finder = new THREE.Mesh(
    new THREE.CylinderGeometry(0.008, 0.01, 0.22, 10),
    navyMat,
  );
  finder.position.set(0.042, 0.052, 0.05);
  finder.rotation.x = Math.PI / 2; // align along Z
  tubeGroup.add(finder);

  const finderDew = new THREE.Mesh(
    new THREE.CylinderGeometry(0.014, 0.014, 0.04, 10),
    brassMat,
  );
  finderDew.position.set(0.042, 0.052, 0.14);
  finderDew.rotation.x = Math.PI / 2;
  tubeGroup.add(finderDew);

  const bracket1 = new THREE.Mesh(
    new THREE.BoxGeometry(0.01, 0.03, 0.01),
    brassMat,
  );
  bracket1.position.set(0.025, 0.028, 0.1);
  tubeGroup.add(bracket1);

  const bracket2 = new THREE.Mesh(
    new THREE.BoxGeometry(0.01, 0.03, 0.01),
    brassMat,
  );
  bracket2.position.set(0.025, 0.028, -0.02);
  tubeGroup.add(bracket2);

  // ─── Mathematical Orientation pointing out the window ───
  // Window opening is in the background, center is approx (-2.5, 1.95, -3.86)
  // Telescope mount head is at (-3.0, 1.54, -2.4)
  // Delta vector: dx = +0.55, dy = +0.41, dz = -1.46
  const dx = 0.55;
  const dy = 0.41;
  const dz = -1.46;
  const h = Math.sqrt(dx * dx + dz * dz);
  const yaw = Math.atan2(dx, dz);
  const pitch = -Math.atan2(dy, h);

  tubeGroup.rotation.y = yaw;
  tubeGroup.rotation.x = pitch;

  telescope.add(tubeGroup);

  // ─── Position Telescope in Back-Left corner next to Window ───
  telescope.position.set(-3.0, 0, -2.4);

  // Traverse and ensure shadows are cast/received properly
  telescope.traverse((child) => {
    if (child.isMesh) {
      child.castShadow = true;
      child.receiveShadow = true;
    }
  });

  state.scene.add(telescope);
  state.cosmos.telescope = telescope;
}

function animateCosmosRoom() {
  if (!state.cosmos.active) return;

  const t = performance.now() * 0.001;

  // Slowly rotate layered starfields in opposite directions (gorgeous parallax)
  if (state.cosmos.starfield1) {
    state.cosmos.starfield1.rotation.y = t * 0.0018;
  }
  if (state.cosmos.starfield2) {
    state.cosmos.starfield2.rotation.y = t * -0.0032;
    state.cosmos.starfield2.rotation.x = Math.sin(t * 0.04) * 0.01;
  }

  // Skybox extremely slow orbital rotation
  if (state.cosmos.skybox) {
    state.cosmos.skybox.rotation.y = t * 0.0004;
  }

  // Cosmic Swirl Nebula slow galaxy disc rotation
  if (state.cosmos.cosmicSwirl) {
    state.cosmos.cosmicSwirl.rotation.z = t * 0.004;
  }

  // ─── Planet 1: Aurelia (Saturn-like Gas Giant) ───
  if (state.cosmos.aureliaMesh) {
    state.cosmos.aureliaMesh.rotation.y = t * 0.012; // slow axial rotation
    const group = state.cosmos.aureliaMesh.parent;
    if (group) {
      group.position.y = Math.sin(t * 0.12) * 0.14; // smooth bobbing
    }
  }
  if (state.cosmos.aureliaGlow) {
    state.cosmos.aureliaGlow.material.opacity = 0.16 + Math.sin(t * 0.8) * 0.06; // glow breathing
  }

  // ─── Planet 4: Selene (Mystical Silver Moon) ───
  if (state.cosmos.seleneMesh) {
    state.cosmos.seleneMesh.rotation.y = t * 0.016;
    const group = state.cosmos.seleneMesh.parent;
    if (group) {
      group.position.y = 6.0 + Math.sin(t * 0.24) * 0.15;
      group.position.x = 5.5 + Math.cos(t * 0.14) * 0.12;
    }
  }
  if (state.cosmos.seleneGlow) {
    state.cosmos.seleneGlow.material.opacity = 0.15 + Math.sin(t * 0.95) * 0.05;
  }

  // 3D Stardust Drift physics
  if (state.cosmos.stardust) {
    const positions = state.cosmos.stardust.geometry.attributes.position.array;
    const speeds = state.cosmos.stardustSpeeds;
    const count = speeds.length;

    for (let i = 0; i < count; i++) {
      // Translate forward towards the room in Z
      positions[i * 3 + 2] += speeds[i].speedZ;

      // Sway in X and Y
      const xOffset = t * speeds[i].swaySpeedX + speeds[i].offsetX;
      const yOffset = t * speeds[i].swaySpeedY + speeds[i].offsetY;
      positions[i * 3] += Math.sin(xOffset) * speeds[i].swayAmpX * 0.01;
      positions[i * 3 + 1] += Math.cos(yOffset) * speeds[i].swayAmpY * 0.01;

      // When deep inside the room (Z > 4.0), wrap back to the deep background (Z = -15.0)
      if (positions[i * 3 + 2] > 4.0) {
        positions[i * 3 + 2] = -15.0;
        positions[i * 3] = -5.0 + Math.random() * 9.0;
        positions[i * 3 + 1] = 0.5 + Math.random() * 5.0;
      }
    }
    state.cosmos.stardust.geometry.attributes.position.needsUpdate = true;
  }

  // Animate shooting stars speed streaks
  if (state.cosmos.shootingStars) {
    state.cosmos.shootingStars.children.forEach((star, i) => {
      star.position.x += star.userData.speed * 12;
      star.position.y -= star.userData.speed * 5;

      // Loop stars back
      if (star.position.x > 0) {
        star.position.x = -15;
        star.position.y = star.userData.baseY + ((i * 0.28 + t) % 5) - 2;
      }
    });
  }

  // Ambient space light gentle shimmer breathing pulse
  if (state.cosmos.ambientStarLight) {
    state.cosmos.ambientStarLight.intensity = 0.75 + Math.sin(t * 0.45) * 0.2;
  }

  // Lens glowing light breathing pulse
  if (state.cosmos.ambientStarLight2) {
    state.cosmos.ambientStarLight2.intensity = 1.6 + Math.sin(t * 1.5) * 0.5;
  }

  // Update stardust particle physics, planet shake decay, explosion ring expansion, and planet rebirth
  updateCosmosEasterEggs(t);
}

/* ─── Cosmos Mode Easter Eggs & Interactive Physics ─── */

state.cosmos.planetClicks = state.cosmos.planetClicks || {
  earth: 0,
  aurelia: 0,
  selene: 0,
  ignis: 0,
  glacia: 0,
};
state.cosmos.planetShakes = state.cosmos.planetShakes || {
  earth: 0,
  aurelia: 0,
  selene: 0,
  ignis: 0,
  glacia: 0,
};
state.cosmos.activeExplosions = state.cosmos.activeExplosions || [];
state.cosmos.rebirthTimers = state.cosmos.rebirthTimers || {
  earth: null,
  aurelia: null,
  selene: null,
  ignis: null,
  glacia: null,
};

// A robust 3D Perlin Noise generator
function createNoise3D() {
  const size = 256;
  const perm = new Uint8Array(size * 2);
  const grads = [
    [1, 1, 0],
    [-1, 1, 0],
    [1, -1, 0],
    [-1, -1, 0],
    [1, 0, 1],
    [-1, 0, 1],
    [1, 0, -1],
    [-1, 0, -1],
    [0, 1, 1],
    [0, -1, 1],
    [0, 1, -1],
    [0, -1, -1],
  ];
  let seed = 7;
  function rand() {
    const x = Math.sin(seed++) * 10000;
    return x - Math.floor(x);
  }
  const p = [];
  for (let i = 0; i < size; i++) p.push(i);
  for (let i = size - 1; i > 0; i--) {
    const j = Math.floor(rand() * (i + 1));
    const temp = p[i];
    p[i] = p[j];
    p[j] = temp;
  }
  for (let i = 0; i < size * 2; i++) {
    perm[i] = p[i % size];
  }

  function lerp(t, a, b) {
    return a + t * (b - a);
  }
  function fade(t) {
    return t * t * t * (t * (t * 6 - 15) + 10);
  }

  return function (x, y, z) {
    const X = Math.floor(x) & 255;
    const Y = Math.floor(y) & 255;
    const Z = Math.floor(z) & 255;

    const xf = x - Math.floor(x);
    const yf = y - Math.floor(y);
    const zf = z - Math.floor(z);

    const u = fade(xf);
    const v = fade(yf);
    const w = fade(zf);

    const aa = perm[perm[X] + Y];
    const ab = perm[perm[X] + Y + 1];
    const ba = perm[perm[X + 1] + Y];
    const bb = perm[perm[X + 1] + Y + 1];

    const aaa = perm[aa + Z];
    const aab = perm[aa + Z + 1];
    const aba = perm[ab + Z];
    const abb = perm[ab + Z + 1];
    const baa = perm[ba + Z];
    const bab = perm[ba + Z + 1];
    const bba = perm[bb + Z];
    const bbb = perm[bb + Z + 1];

    const gradSize = grads.length;
    const gAAA = grads[aaa % gradSize];
    const gAAB = grads[aab % gradSize];
    const gABA = grads[aba % gradSize];
    const gABB = grads[abb % gradSize];
    const gBAA = grads[baa % gradSize];
    const gBAB = grads[bab % gradSize];
    const gBBA = grads[bba % gradSize];
    const gBBB = grads[bbb % gradSize];

    const dAAA = gAAA[0] * xf + gAAA[1] * yf + gAAA[2] * zf;
    const dAAB = gAAB[0] * xf + gAAB[1] * yf + gAAB[2] * (zf - 1);
    const dABA = gABA[0] * xf + gABA[1] * (yf - 1) + gABA[2] * zf;
    const dABB = gABB[0] * xf + gABB[1] * (yf - 1) + gABB[2] * (zf - 1);
    const dBAA = gBAA[0] * (xf - 1) + gBAA[1] * yf + gBAA[2] * zf;
    const dBAB = gBAB[0] * (xf - 1) + gBAB[1] * yf + gBAB[2] * (zf - 1);
    const dBBA = gBBA[0] * (xf - 1) + gBBA[1] * (yf - 1) + gBBA[2] * zf;
    const dBBB = gBBB[0] * (xf - 1) + gBBB[1] * (yf - 1) + gBBB[2] * (zf - 1);

    const x1 = lerp(u, dAAA, dBAA);
    const x2 = lerp(u, dABA, dBBA);
    const x3 = lerp(u, dAAB, dBAB);
    const x4 = lerp(u, dABB, dBBB);

    const y1 = lerp(v, x1, x2);
    const y2 = lerp(v, x3, x4);

    return lerp(w, y1, y2);
  };
}
const noise3D = createNoise3D();

function fbm3D(x, y, z, octaves) {
  let value = 0;
  let amplitude = 1.0;
  let frequency = 1.0;
  let maxVal = 0;
  for (let i = 0; i < octaves; i++) {
    value += amplitude * noise3D(x * frequency, y * frequency, z * frequency);
    maxVal += amplitude;
    amplitude *= 0.5;
    frequency *= 2.0;
  }
  return value / maxVal;
}

function createDetailedEarthTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(1024, 512);
  const data = imgData.data;

  for (let y = 0; y < 512; y++) {
    const latNorm = Math.abs(y - 256) / 256;
    const phi = (y / 512) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 1024; x++) {
      const theta = (x / 1024) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 3.2, sy * 3.2, sz * 3.2, 5);
      const idx = (y * 1024 + x) * 4;

      const iceFactor = latNorm + n * 0.08;
      if (iceFactor > 0.82) {
        data[idx] = 248; // R
        data[idx + 1] = 252; // G
        data[idx + 2] = 255; // B
        data[idx + 3] = 255;
        continue;
      }

      if (n > -0.05) {
        let r, g, b;
        const dn = fbm3D(sx * 100, sy * 100, sz * 100, 2) * 12;

        if (latNorm < 0.25) {
          const desertNoise = fbm3D(sx * 8, sy * 8, sz * 8, 2);
          if (desertNoise > 0.1) {
            r = 205 + dn;
            g = 170 + dn;
            b = 118 + dn;
          } else {
            r = 18 + dn;
            g = 78 + dn;
            b = 28 + dn;
          }
        } else if (latNorm < 0.65) {
          r = 25 + dn;
          g = 98 + dn;
          b = 35 + dn;
        } else {
          r = 72 + dn;
          g = 82 + dn;
          b = 50 + dn;
        }

        if (n < -0.01) {
          const t = (n + 0.05) / 0.04;
          r = r * t + 225 * (1.0 - t);
          g = g * t + 195 * (1.0 - t);
          b = b * t + 140 * (1.0 - t);
        }

        data[idx] = Math.max(0, Math.min(255, r));
        data[idx + 1] = Math.max(0, Math.min(255, g));
        data[idx + 2] = Math.max(0, Math.min(255, b));
        data[idx + 3] = 255;
      } else {
        const depth = Math.min(1.0, Math.max(0.0, (-0.05 - n) * 4.0));
        const r = Math.floor(6 * (1 - depth) + 3 * depth);
        const g = Math.floor(105 * (1 - depth) + 12 * depth);
        const b = Math.floor(165 * (1 - depth) + 40 * depth);

        data[idx] = r;
        data[idx + 1] = g;
        data[idx + 2] = b;
        data[idx + 3] = 255;
      }
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createDetailedEarthRoughnessTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(512, 256);
  const data = imgData.data;

  for (let y = 0; y < 256; y++) {
    const latNorm = Math.abs(y - 128) / 128;
    const phi = (y / 256) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 512; x++) {
      const theta = (x / 512) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 3.2, sy * 3.2, sz * 3.2, 4);
      const idx = (y * 512 + x) * 4;

      let val;
      const iceFactor = latNorm + n * 0.08;

      if (iceFactor > 0.82) {
        val = 110;
      } else if (n > -0.05) {
        val = 225;
      } else {
        val = 25;
      }

      data[idx] = val;
      data[idx + 1] = val;
      data[idx + 2] = val;
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createDetailedEarthBumpTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(512, 256);
  const data = imgData.data;

  for (let y = 0; y < 256; y++) {
    const latNorm = Math.abs(y - 128) / 128;
    const phi = (y / 256) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 512; x++) {
      const theta = (x / 512) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 3.2, sy * 3.2, sz * 3.2, 4);
      const idx = (y * 512 + x) * 4;

      let height = 0;
      const iceFactor = latNorm + n * 0.08;

      if (iceFactor > 0.82) {
        height = 55;
      } else if (n > -0.05) {
        const normalizedLand = (n + 0.05) / 1.05;
        const peakNoise = Math.abs(fbm3D(sx * 22, sy * 22, sz * 22, 2));
        height = 60 + normalizedLand * 140 + peakNoise * 55;
        height = Math.max(0, Math.min(255, height));
      } else {
        height = 0;
      }

      data[idx] = height;
      data[idx + 1] = height;
      data[idx + 2] = height;
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createDetailedEarthEmissiveTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(512, 256);
  const data = imgData.data;

  for (let y = 0; y < 256; y++) {
    const latNorm = Math.abs(y - 128) / 128;
    const phi = (y / 256) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 512; x++) {
      const theta = (x / 512) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 3.2, sy * 3.2, sz * 3.2, 4);
      const idx = (y * 512 + x) * 4;

      if (latNorm <= 0.7 && n > -0.02 && n < 0.22) {
        const roadNoise = fbm3D(sx * 140, sy * 140, sz * 140, 2);
        if (roadNoise > 0.45) {
          const brightness = Math.floor(130 + (roadNoise - 0.45) * 225);
          data[idx] = Math.min(255, brightness);
          data[idx + 1] = Math.min(255, Math.floor(brightness * 0.85));
          data[idx + 2] = Math.min(255, Math.floor(brightness * 0.5));
          data[idx + 3] = 255;
          continue;
        }
      }

      data[idx] = 0;
      data[idx + 1] = 0;
      data[idx + 2] = 0;
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createDetailedEarthCloudsTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 512;
  canvas.height = 256;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(512, 256);
  const data = imgData.data;

  for (let y = 0; y < 256; y++) {
    const windBand = Math.sin((y / 256) * Math.PI);
    const phi = (y / 256) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 512; x++) {
      const theta = (x / 512) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 4.5, sy * 4.5, sz * 4.5, 4);
      const val = n * 0.5 + 0.5;

      if (val > 0.51) {
        const idx = (y * 512 + x) * 4;
        const alpha = Math.min(
          255,
          Math.floor((val - 0.51) * 3.5 * 255 * windBand),
        );
        data[idx] = 255;
        data[idx + 1] = 255;
        data[idx + 2] = 255;
        data[idx + 3] = alpha;
      }
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createSilverMoonBumpTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(1024, 512);
  const data = imgData.data;

  for (let y = 0; y < 512; y++) {
    const phi = (y / 512) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 1024; x++) {
      const theta = (x / 1024) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 6, sy * 6, sz * 6, 4);
      const idx = (y * 1024 + x) * 4;

      const craterNoise = Math.abs(fbm3D(sx * 18, sy * 18, sz * 18, 3));
      let height = 128 + n * 45;
      if (craterNoise > 0.35) {
        height -= 60;
      }
      height = Math.max(0, Math.min(255, height));

      data[idx] = height;
      data[idx + 1] = height;
      data[idx + 2] = height;
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createVolcanicTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(1024, 512);
  const data = imgData.data;

  for (let y = 0; y < 512; y++) {
    const phi = (y / 512) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 1024; x++) {
      const theta = (x / 1024) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const cracks = Math.abs(fbm3D(sx * 35, sy * 35, sz * 35, 2));
      const idx = (y * 1024 + x) * 4;

      let r, g, b;

      if (cracks > 0.46) {
        const heat = (cracks - 0.46) / 0.54;
        r = 255;
        g = Math.floor(55 + heat * 180);
        b = 0;
      } else {
        const bn = fbm3D(sx * 80, sy * 80, sz * 80, 2) * 12;
        r = 28 + bn;
        g = 22 + bn;
        b = 20 + bn;
      }

      data[idx] = Math.max(0, Math.min(255, r));
      data[idx + 1] = Math.max(0, Math.min(255, g));
      data[idx + 2] = Math.max(0, Math.min(255, b));
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createVolcanicBumpTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(1024, 512);
  const data = imgData.data;

  for (let y = 0; y < 512; y++) {
    const phi = (y / 512) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 1024; x++) {
      const theta = (x / 1024) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 4, sy * 4, sz * 4, 4);
      const cracks = Math.abs(fbm3D(sx * 35, sy * 35, sz * 35, 2));

      const idx = (y * 1024 + x) * 4;

      let height = 120;
      if (cracks > 0.46) {
        height = 20;
      } else {
        height = 100 + n * 80 + Math.random() * 15;
      }

      data[idx] = height;
      data[idx + 1] = height;
      data[idx + 2] = height;
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createVolcanicEmissiveTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(1024, 512);
  const data = imgData.data;

  for (let y = 0; y < 512; y++) {
    const phi = (y / 512) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 1024; x++) {
      const theta = (x / 1024) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const cracks = Math.abs(fbm3D(sx * 35, sy * 35, sz * 35, 2));
      const idx = (y * 1024 + x) * 4;

      if (cracks > 0.46) {
        const heat = (cracks - 0.46) / 0.54;
        data[idx] = 255;
        data[idx + 1] = Math.floor(55 + heat * 180);
        data[idx + 2] = 0;
        data[idx + 3] = 255;
      } else {
        data[idx] = 0;
        data[idx + 1] = 0;
        data[idx + 2] = 0;
        data[idx + 3] = 255;
      }
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createIceGiantTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(1024, 512);
  const data = imgData.data;

  for (let y = 0; y < 512; y++) {
    const phi = (y / 512) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 1024; x++) {
      const theta = (x / 1024) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 3, sy * 3.5, sz * 3, 5);
      const swirl = fbm3D(sx * 15, sy * 15, sz * 15, 3);
      const idx = (y * 1024 + x) * 4;

      const ratio = Math.min(
        1.0,
        Math.max(0.0, (n + 0.4) * 0.9 + swirl * 0.15),
      );
      const r = Math.floor(10 * (1 - ratio) + 180 * ratio);
      const g = Math.floor(65 * (1 - ratio) + 235 * ratio);
      const b = Math.floor(115 * (1 - ratio) + 250 * ratio);

      data[idx] = r;
      data[idx + 1] = g;
      data[idx + 2] = b;
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createIceGiantBumpTexture() {
  const canvas = document.createElement("canvas");
  canvas.width = 1024;
  canvas.height = 512;
  const ctx = canvas.getContext("2d");

  const imgData = ctx.createImageData(1024, 512);
  const data = imgData.data;

  for (let y = 0; y < 512; y++) {
    const phi = (y / 512) * Math.PI;
    const sy = Math.cos(phi);
    const sinPhi = Math.sin(phi);

    for (let x = 0; x < 1024; x++) {
      const theta = (x / 1024) * Math.PI * 2;
      const sx = sinPhi * Math.cos(theta);
      const sz = sinPhi * Math.sin(theta);

      const n = fbm3D(sx * 12, sy * 12, sz * 12, 3);
      const idx = (y * 1024 + x) * 4;

      const height = Math.floor(128 + n * 35);
      data[idx] = height;
      data[idx + 1] = height;
      data[idx + 2] = height;
      data[idx + 3] = 255;
    }
  }

  ctx.putImageData(imgData, 0, 0);
  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.ClampToEdgeWrapping;
  return texture;
}

function createDetailedAtmosphericGlow(size, colorHex, sunLight, camera) {
  const geo = new THREE.SphereGeometry(size * 1.025, 64, 64);
  const color = new THREE.Color(colorHex);
  const mat = new THREE.ShaderMaterial({
    vertexShader: `
            varying vec3 vWorldNormal;
            varying vec3 vWorldPosition;
            void main() {
                vWorldNormal = normalize(vec3(modelMatrix * vec4(normal, 0.0)));
                vWorldPosition = vec3(modelMatrix * vec4(position, 1.0));
                gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
            }
        `,
    fragmentShader: `
            varying vec3 vWorldNormal;
            varying vec3 vWorldPosition;
            uniform vec3 cameraPosition;
            uniform vec3 sunPosition;
            uniform vec3 glowColor;
            void main() {
                vec3 viewDir = normalize(cameraPosition - vWorldPosition);
                vec3 sunDir = normalize(sunPosition - vWorldPosition);
                
                float viewDotNormal = dot(viewDir, vWorldNormal);
                float fresnel = pow(1.0 - max(0.0, viewDotNormal), 3.0);
                
                float sunDotNormal = dot(sunDir, vWorldNormal);
                float sunScatter = smoothstep(-0.35, 0.25, sunDotNormal);
                
                vec3 color = glowColor * fresnel * sunScatter * 1.6;
                gl_FragColor = vec4(color, fresnel * sunScatter);
            }
        `,
    uniforms: {
      glowColor: { value: color },
      sunPosition: { value: sunLight.position },
      cameraPosition: { value: camera.position },
    },
    blending: THREE.AdditiveBlending,
    side: THREE.BackSide,
    transparent: true,
    depthWrite: false,
  });
  return new THREE.Mesh(geo, mat);
}

function applyEmissiveModifier(material, sunLight) {
  material.onBeforeCompile = (shader) => {
    shader.uniforms.sunPosition = { value: sunLight.position };
    shader.vertexShader = shader.vertexShader.replace(
      "#include <common>",
      `#include <common>
             varying vec3 vWorldNormalForEmissive;
             varying vec3 vWorldPositionForEmissive;`,
    );
    shader.vertexShader = shader.vertexShader.replace(
      "#include <beginnormal_vertex>",
      `#include <beginnormal_vertex>
             vWorldNormalForEmissive = normalize(vec3(modelMatrix * vec4(normal, 0.0)));`,
    );
    shader.vertexShader = shader.vertexShader.replace(
      "#include <begin_vertex>",
      `#include <begin_vertex>
             vWorldPositionForEmissive = vec3(modelMatrix * vec4(position, 1.0));`,
    );
    shader.fragmentShader = shader.fragmentShader.replace(
      "#include <common>",
      `#include <common>
             uniform vec3 sunPosition;
             varying vec3 vWorldNormalForEmissive;
             varying vec3 vWorldPositionForEmissive;`,
    );
    shader.fragmentShader = shader.fragmentShader.replace(
      "#include <emissivemap_fragment>",
      `#include <emissivemap_fragment>
             vec3 sunDir = normalize(sunPosition - vWorldPositionForEmissive);
             float sunDot = dot(vWorldNormalForEmissive, sunDir);
             float nightFactor = smoothstep(0.1, -0.2, sunDot);
             totalEmissiveRadiance *= nightFactor;`,
    );
  };
}

function switchViewfinderPlanet(planetName) {
  if (!state.cosmos.earth) return;
  const { scene, earthMesh, cloudMesh, glow, sunLight } = state.cosmos.earth;
  if (!earthMesh) return;

  const labels = {
    earth: "ĐANG QUAN SÁT: TRÁI ĐẤT (TERRA)",
    aurelia: "ĐANG QUAN SÁT: TINH CẦU AURELIA",
    selene: "ĐANG QUAN SÁT: MẶT TRĂNG SELENE",
    ignis: "ĐANG QUAN SÁT: HÀNH TINH LỬA IGNIS",
    glacia: "ĐANG QUAN SÁT: HÀNH TINH BĂNG GLACIA",
  };

  const labelEl = document.querySelector(".viewfinder-label");
  if (labelEl) labelEl.textContent = labels[planetName] || "";

  const sublabels = {
    earth: "Quần đảo Hoàng Sa và Trường Sa thuộc chủ quyền Việt Nam 🇻🇳",
    aurelia: "Tinh cầu khí khổng lồ với những cơn bão plasma rực rỡ",
    selene: "Vệ tinh tự nhiên tĩnh lặng phủ đầy cát bụi vũ trụ",
    ignis: "Hành tinh núi lửa cuồn cuộn dung nham nóng bỏng",
    glacia: "Thế giới băng giá vô tận với lớp vỏ đóng băng vĩnh cửu",
  };

  const sublabelEl = document.querySelector(".viewfinder-sublabel");
  if (sublabelEl) {
    sublabelEl.textContent = sublabels[planetName] || "";
    if (planetName === "earth") {
      sublabelEl.style.color = "#4ade80";
      sublabelEl.style.textShadow = "0 0 10px rgba(74, 222, 128, 0.6)";
    } else {
      sublabelEl.style.color = "#00ffd2";
      sublabelEl.style.textShadow = "0 0 8px rgba(0, 255, 210, 0.5)";
    }
  }

  // Clear previous textures and materials
  if (earthMesh.material) {
    const mat = earthMesh.material;
    if (mat.map) mat.map.dispose();
    if (mat.roughnessMap) mat.roughnessMap.dispose();
    if (mat.bumpMap) mat.bumpMap.dispose();
    if (mat.emissiveMap) mat.emissiveMap.dispose();
    mat.dispose();
  }

  if (state.cosmos.earth.ringsMesh) {
    scene.remove(state.cosmos.earth.ringsMesh);
    state.cosmos.earth.ringsMesh.geometry.dispose();
    if (state.cosmos.earth.ringsMesh.material.map)
      state.cosmos.earth.ringsMesh.material.map.dispose();
    state.cosmos.earth.ringsMesh.material.dispose();
    state.cosmos.earth.ringsMesh = null;
  }

  let newMat;
  let showClouds = false;
  let glowColor = 0x00a2ff;

  if (planetName === "earth") {
    const earthTex = createDetailedEarthTexture();
    const earthRoughnessTex = createDetailedEarthRoughnessTexture();
    const earthBumpTex = createDetailedEarthBumpTexture();
    const earthEmissiveTex = createDetailedEarthEmissiveTexture();

    newMat = new THREE.MeshStandardMaterial({
      map: earthTex,
      roughnessMap: earthRoughnessTex,
      bumpMap: earthBumpTex,
      bumpScale: 0.04,
      emissiveMap: earthEmissiveTex,
      emissive: 0xffffff,
      emissiveIntensity: 0.95,
      roughness: 1.0,
      metalness: 0.1,
    });
    applyEmissiveModifier(newMat, sunLight);
    showClouds = true;
    glowColor = 0x88c2ff;
  } else if (planetName === "aurelia") {
    const aureliaTex = createGasGiantTexture();
    newMat = new THREE.MeshStandardMaterial({
      map: aureliaTex,
      roughness: 0.8,
      metalness: 0.1,
    });

    const ringTexture = createRingTexture();
    const ringGeo = new THREE.RingGeometry(2.0, 3.6, 64);
    const pos = ringGeo.attributes.position;
    const uvs = ringGeo.attributes.uv;
    for (let i = 0; i < pos.count; i++) {
      const x = pos.getX(i);
      const y = pos.getY(i);
      const r = Math.sqrt(x * x + y * y);
      const u = (r - 2.0) / (3.6 - 2.0);
      uvs.setXY(i, u, 0.5);
    }
    uvs.needsUpdate = true;

    const ringMat = new THREE.MeshStandardMaterial({
      map: ringTexture,
      side: THREE.DoubleSide,
      transparent: true,
      opacity: 0.85,
      roughness: 0.6,
      depthWrite: false,
    });
    const ringsMesh = new THREE.Mesh(ringGeo, ringMat);
    ringsMesh.rotation.x = Math.PI / 2.3;
    ringsMesh.rotation.y = 0.1;
    scene.add(ringsMesh);
    state.cosmos.earth.ringsMesh = ringsMesh;

    glowColor = 0xffcda0;
  } else if (planetName === "selene") {
    const seleneTex = createSilverMoonTexture();
    const seleneBumpTex = createSilverMoonBumpTexture();
    newMat = new THREE.MeshStandardMaterial({
      map: seleneTex,
      bumpMap: seleneBumpTex,
      bumpScale: 0.03,
      roughness: 0.85,
      metalness: 0.05,
    });
    glowColor = 0xd0e0f0;
  } else if (planetName === "ignis") {
    const volcanicTex = createVolcanicTexture();
    const volcanicBumpTex = createVolcanicBumpTexture();
    const volcanicEmissiveTex = createVolcanicEmissiveTexture();
    newMat = new THREE.MeshStandardMaterial({
      map: volcanicTex,
      bumpMap: volcanicBumpTex,
      bumpScale: 0.05,
      emissiveMap: volcanicEmissiveTex,
      emissive: 0xffffff,
      emissiveIntensity: 1.5,
      roughness: 0.8,
      metalness: 0.2,
    });
    applyEmissiveModifier(newMat, sunLight);
    glowColor = 0xff3300;
  } else if (planetName === "glacia") {
    const iceTex = createIceGiantTexture();
    const iceBumpTex = createIceGiantBumpTexture();
    newMat = new THREE.MeshStandardMaterial({
      map: iceTex,
      bumpMap: iceBumpTex,
      bumpScale: 0.02,
      roughness: 0.25,
      metalness: 0.1,
    });
    glowColor = 0x80deea;
  }

  earthMesh.material = newMat;
  if (cloudMesh) cloudMesh.visible = showClouds;
  if (glow && glow.material && glow.material.uniforms.glowColor) {
    glow.material.uniforms.glowColor.value.setHex(glowColor);
  }
}

function triggerTelescopeEasterEgg() {
  const el = document.getElementById("telescopeViewfinder");
  const container = document.getElementById("earthCanvasContainer");
  const btnClose = document.getElementById("closeTelescopeViewBtn");
  if (!el || !container || !btnClose) return;

  el.style.display = "flex";
  requestAnimationFrame(() => el.classList.add("active"));

  if (state.orbitControls) state.orbitControls.enabled = false;

  state.cosmos.earth = state.cosmos.earth || {};
  container.innerHTML = "";

  const width = container.clientWidth || window.innerWidth * 0.6;
  const height = container.clientHeight || window.innerHeight * 0.6;

  const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
  renderer.setSize(width, height);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  container.appendChild(renderer.domElement);
  state.cosmos.earth.renderer = renderer;

  const scene = new THREE.Scene();
  const camera = new THREE.PerspectiveCamera(40, width / height, 0.1, 100);
  camera.position.set(0, 0, 5.2);
  state.cosmos.earth.scene = scene;
  state.cosmos.earth.camera = camera;

  const controls = new OrbitControls(camera, renderer.domElement);
  controls.enableDamping = true;
  controls.dampingFactor = 0.05;
  controls.minDistance = 2.8;
  controls.maxDistance = 8.0;
  state.cosmos.earth.controls = controls;

  const sunLight = new THREE.DirectionalLight(0xffffff, 2.5);
  sunLight.position.set(5, 3, 5);
  scene.add(sunLight);
  state.cosmos.earth.sunLight = sunLight;

  const blueFillLight = new THREE.DirectionalLight(0x3b82f6, 1.2);
  blueFillLight.position.set(-5, -2, -3);
  scene.add(blueFillLight);

  const spaceAmbi = new THREE.AmbientLight(0x0c1020, 0.65);
  scene.add(spaceAmbi);

  const earthGeo = new THREE.SphereGeometry(1.6, 64, 64);
  const earthMat = new THREE.MeshStandardMaterial({ color: 0x000000 });
  const earthMesh = new THREE.Mesh(earthGeo, earthMat);
  scene.add(earthMesh);
  state.cosmos.earth.earthMesh = earthMesh;

  const cloudGeo = new THREE.SphereGeometry(1.62, 64, 64);
  const cloudTex = createDetailedEarthCloudsTexture();
  const cloudMat = new THREE.MeshStandardMaterial({
    map: cloudTex,
    transparent: true,
    blending: THREE.NormalBlending,
    depthWrite: false,
  });
  const cloudMesh = new THREE.Mesh(cloudGeo, cloudMat);
  scene.add(cloudMesh);
  state.cosmos.earth.cloudMesh = cloudMesh;

  const glow = createDetailedAtmosphericGlow(1.65, 0x88c2ff, sunLight, camera);
  scene.add(glow);
  state.cosmos.earth.glow = glow;

  // Load default planet (Earth)
  switchViewfinderPlanet("earth");

  // Click handler for planet switching buttons
  const btns = el.querySelectorAll(".planet-select-btn");
  btns.forEach((btn) => {
    btn.onclick = function () {
      btns.forEach((b) => b.classList.remove("active"));
      btn.classList.add("active");
      switchViewfinderPlanet(btn.getAttribute("data-planet"));
    };
  });

  let reqId;
  function renderEarth() {
    reqId = requestAnimationFrame(renderEarth);

    if (earthMesh) earthMesh.rotation.y += 0.0016;
    if (cloudMesh) {
      cloudMesh.rotation.y += 0.0024;
      cloudMesh.rotation.x += 0.0004;
    }
    if (state.cosmos.earth.ringsMesh) {
      state.cosmos.earth.ringsMesh.rotation.z += 0.001;
    }

    controls.update();

    if (glow && glow.material && glow.material.uniforms.cameraPosition) {
      glow.material.uniforms.cameraPosition.value.copy(camera.position);
    }

    renderer.render(scene, camera);
  }
  renderEarth();
  state.cosmos.earth.reqId = reqId;

  function handleResize() {
    if (!container || !camera || !renderer) return;
    const w = container.clientWidth;
    const h = container.clientHeight;
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h);
  }
  window.addEventListener("resize", handleResize);
  state.cosmos.earth.resizeHandler = handleResize;

  btnClose.onclick = function () {
    closeTelescopeEasterEgg();
  };
}

function closeTelescopeEasterEgg() {
  const el = document.getElementById("telescopeViewfinder");
  if (!el || !el.classList.contains("active")) return;

  el.classList.remove("active");
  setTimeout(() => {
    el.style.display = "none";
  }, 400);

  if (state.orbitControls) state.orbitControls.enabled = true;

  const earth = state.cosmos.earth;
  if (earth) {
    if (earth.reqId) cancelAnimationFrame(earth.reqId);
    if (earth.resizeHandler)
      window.removeEventListener("resize", earth.resizeHandler);
    if (earth.controls) earth.controls.dispose();

    if (earth.scene) {
      earth.scene.traverse((child) => {
        if (child.isMesh) {
          child.geometry.dispose();
          if (child.material.map) child.material.map.dispose();
          if (child.material.roughnessMap)
            child.material.roughnessMap.dispose();
          if (child.material.bumpMap) child.material.bumpMap.dispose();
          if (child.material.emissiveMap) child.material.emissiveMap.dispose();
          child.material.dispose();
        }
      });
    }

    const container = document.getElementById("earthCanvasContainer");
    if (container) container.innerHTML = "";

    state.cosmos.earth = null;
  }
}

function triggerPlanetClick(planetName) {
  const clicks = state.cosmos.planetClicks;
  if (clicks[planetName] === -1) return; // Ignore clicks during explosion sequence

  clicks[planetName] = (clicks[planetName] || 0) + 1;
  const count = clicks[planetName];

  if (count < 5) {
    // High-frequency vibration amplitude increases on each consecutive click
    state.cosmos.planetShakes[planetName] = count * 0.16;

    // Exponentially increase room/camera shake on standard damage hits
    state.cosmos.roomShake = Math.max(
      state.cosmos.roomShake || 0,
      Math.pow(count, 1.5) * 0.05,
    );

    // Temporarily brighten glow shell intensity on impact
    const glow =
      planetName === "aurelia"
        ? state.cosmos.aureliaGlow
        : state.cosmos.seleneGlow;
    if (glow) {
      glow.material.opacity = 0.9;
    }

    const warnings = [
      `Xung kích hấp thụ trên ${planetName === "aurelia" ? "Aurelia" : "Selene"} tăng cao!`,
      `Cảnh báo: Áp suất khí quyển đang gia tăng!`,
      `Lớp lõi địa chất bắt đầu nứt vỡ cơ học!`,
      `CẢNH BÁO QUÁ TẢI: NĂNG LƯỢNG LÕI VŨ TRỤ TRUNG HÒA!`,
    ];
    showToast(warnings[count - 1]);
  } else {
    // Explode the target
    explodePlanet(planetName);
  }
}

function explodePlanet(planetName) {
  const mesh =
    planetName === "aurelia"
      ? state.cosmos.aureliaMesh
      : state.cosmos.seleneMesh;
  if (!mesh) return;

  const group = mesh.parent;
  if (!group) return;

  showToast(
    `⚠️ PHẢN ỨNG PHÂN RÃ: Nhân tinh cầu ${planetName === "aurelia" ? "Aurelia" : "Selene"} mất ổn định cực độ!`,
  );

  // Flag as in-explosion to ignore spam clicks
  const clicks = state.cosmos.planetClicks;
  clicks[planetName] = -1;

  // Save original scale
  const originalScale = group.scale.clone();
  const glow =
    planetName === "aurelia"
      ? state.cosmos.aureliaGlow
      : state.cosmos.seleneGlow;
  const originalGlowScale = glow ? glow.scale.clone() : null;

  const startTime = performance.now();
  const duration = 750; // 0.75 seconds of dramatic swelling & vibrating

  function charge() {
    const elapsed = performance.now() - startTime;
    const p = Math.min(1.0, elapsed / duration);

    if (p < 1.0) {
      // Swell up to 1.35x scale
      const s = 1.0 + p * 0.35;
      group.scale.set(
        originalScale.x * s,
        originalScale.y * s,
        originalScale.z * s,
      );

      // Shuddering shake gets faster/stronger
      state.cosmos.planetShakes[planetName] = 0.1 + p * 0.4;
      state.cosmos.roomShake = Math.max(
        state.cosmos.roomShake || 0,
        0.15 + p * 0.25,
      );

      if (glow) {
        const gs = 1.0 + p * 0.5;
        glow.scale.set(
          originalGlowScale.x * gs,
          originalGlowScale.y * gs,
          originalGlowScale.z * gs,
        );
        glow.material.opacity = 0.9 + p * 0.1;
      }

      requestAnimationFrame(charge);
    } else {
      // Restore scale, reset click counter and launch actual blast!
      group.scale.copy(originalScale);
      if (glow && originalGlowScale) glow.scale.copy(originalGlowScale);

      clicks[planetName] = 0;
      state.cosmos.planetShakes[planetName] = 0;

      triggerActualExplosion(planetName, group);
    }
  }
  charge();
}

function triggerActualExplosion(planetName, group) {
  showToast(
    `BÙM! ${planetName === "aurelia" ? "Tinh cầu Aurelia" : "Mặt trăng Selene"} đã phát nổ dữ dội!`,
  );

  // Peak massive camera impact shake
  state.cosmos.roomShake = 1.25;

  // 1. Hide target planet components (rings, atmosphere)
  group.visible = false;

  // Determine world space coordinates
  const worldPos = new THREE.Vector3();
  group.getWorldPosition(worldPos);

  // 2. Expand additively blended shockwave ring
  const ringGeo = new THREE.RingGeometry(0.1, 0.45, 32);
  const ringMat = new THREE.MeshBasicMaterial({
    color: planetName === "aurelia" ? 0xffca28 : 0x80deea,
    side: THREE.DoubleSide,
    transparent: true,
    opacity: 0.95,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
  });
  const shockwave = new THREE.Mesh(ringGeo, ringMat);
  shockwave.position.copy(worldPos);
  shockwave.lookAt(state.camera.position);
  state.scene.add(shockwave);

  // 3. Instantiate particle-physics sparks (130 active particles)
  const particleCount = 130;
  const geometry = new THREE.BufferGeometry();
  const positions = new Float32Array(particleCount * 3);
  const velocities = [];
  const colors = new Float32Array(particleCount * 3);
  const sizes = new Float32Array(particleCount);

  const baseCol =
    planetName === "aurelia"
      ? new THREE.Color(0xff9f00)
      : new THREE.Color(0xa2f5ff);
  const hotCol = new THREE.Color(0xffffff);

  for (let i = 0; i < particleCount; i++) {
    positions[i * 3] = worldPos.x;
    positions[i * 3 + 1] = worldPos.y;
    positions[i * 3 + 2] = worldPos.z;

    // Spherical 3D directional vector distributions
    const theta = Math.random() * Math.PI * 2;
    const phi = Math.acos(Math.random() * 2 - 1);
    const speed = 0.12 + Math.random() * 0.28;

    velocities.push({
      x: Math.sin(phi) * Math.cos(theta) * speed,
      y: Math.sin(phi) * Math.sin(theta) * speed,
      z: Math.cos(phi) * speed,
    });

    // Color decay logic
    const ratio = Math.random();
    const col = baseCol.clone().lerp(hotCol, ratio);
    colors[i * 3] = col.r;
    colors[i * 3 + 1] = col.g;
    colors[i * 3 + 2] = col.b;

    sizes[i] = 0.08 + Math.random() * 0.22;
  }

  geometry.setAttribute("position", new THREE.BufferAttribute(positions, 3));
  geometry.setAttribute("color", new THREE.BufferAttribute(colors, 3));
  geometry.setAttribute("size", new THREE.BufferAttribute(sizes, 1));

  const starTexture = createGlowingStarTexture();
  const mat = new THREE.PointsMaterial({
    size: 0.26,
    sizeAttenuation: true,
    vertexColors: true,
    transparent: true,
    opacity: 1.0,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
    map: starTexture,
  });

  const particles = new THREE.Points(geometry, mat);
  state.scene.add(particles);

  // Track active explosion components for updates in main loops
  state.cosmos.activeExplosions.push({
    shockwave: shockwave,
    particles: particles,
    velocities: velocities,
    timeCreated: performance.now() * 0.001,
    lifeSpan: 1.8,
  });

  // 4. Set slow cosmic rebirth binding loop timer (10 seconds)
  if (state.cosmos.rebirthTimers[planetName]) {
    clearTimeout(state.cosmos.rebirthTimers[planetName]);
  }

  state.cosmos.rebirthTimers[planetName] = setTimeout(() => {
    rebirthPlanet(planetName, group);
  }, 10000);
}

function rebirthPlanet(planetName, group) {
  showToast(
    `Lực lượng liên kết hấp cực... ${planetName === "aurelia" ? "Aurelia" : "Selene"} đang tái cấu trúc!`,
  );

  // Reset scales to zero and enable visibility
  group.scale.set(0.01, 0.01, 0.01);
  group.visible = true;

  // Track rebirth transitions in update loop
  group.userData.rebirthTimeStart = performance.now() * 0.001;
  group.userData.rebirthActive = true;
}

function updateCosmosEasterEggs(t) {
  // 1. Shaking vibration updates (clicks 1-4)
  const shakes = state.cosmos.planetShakes;
  ["aurelia", "selene"].forEach((name) => {
    const amp = shakes[name];
    const mesh =
      name === "aurelia" ? state.cosmos.aureliaMesh : state.cosmos.seleneMesh;
    if (mesh && amp > 0.001) {
      const group = mesh.parent;
      if (group) {
        // Apply a random shaking offset
        group.position.x += (Math.random() - 0.5) * amp;
        group.position.y += (Math.random() - 0.5) * amp;
        group.position.z += (Math.random() - 0.5) * amp;

        // Exponential vibration decays
        shakes[name] *= 0.9;

        // Return to base coordinates cleanly when shake is complete
        if (shakes[name] < 0.001) {
          shakes[name] = 0;
          if (name === "aurelia") {
            group.position.set(0, 0, 0);
          } else {
            group.position.set(5.5, 6.0, -8.0);
          }
        }
      }
    }
  });

  // 2. Active explosion ring & spark decays
  const explosions = state.cosmos.activeExplosions;
  for (let i = explosions.length - 1; i >= 0; i--) {
    const exp = explosions[i];
    const age = performance.now() * 0.001 - exp.timeCreated;
    const progress = age / exp.lifeSpan;

    if (progress >= 1.0) {
      // Delete explosion components cleanly
      state.scene.remove(exp.shockwave);
      exp.shockwave.geometry.dispose();
      exp.shockwave.material.dispose();

      state.scene.remove(exp.particles);
      exp.particles.geometry.dispose();
      exp.particles.material.dispose();

      explosions.splice(i, 1);
    } else {
      // Shockwave expand and fade
      const scale = 1.0 + progress * 22.0;
      exp.shockwave.scale.set(scale, scale, 1.0);
      exp.shockwave.material.opacity = 1.0 - progress;

      // Move debris outward with air friction drag deceleration
      const positions = exp.particles.geometry.attributes.position.array;
      const vels = exp.velocities;
      const count = vels.length;

      for (let pIdx = 0; pIdx < count; pIdx++) {
        const vel = vels[pIdx];
        positions[pIdx * 3] += vel.x;
        positions[pIdx * 3 + 1] += vel.y;
        positions[pIdx * 3 + 2] += vel.z;

        vel.x *= 0.95;
        vel.y *= 0.95;
        vel.z *= 0.95;
      }
      exp.particles.geometry.attributes.position.needsUpdate = true;
      exp.particles.material.opacity = Math.pow(1.0 - progress, 1.4);
    }
  }

  // 3. Gravity rebirth scale transitions (3-second duration)
  ["aurelia", "selene"].forEach((name) => {
    const mesh =
      name === "aurelia" ? state.cosmos.aureliaMesh : state.cosmos.seleneMesh;
    if (mesh) {
      const group = mesh.parent;
      if (group && group.userData.rebirthActive) {
        const age = performance.now() * 0.001 - group.userData.rebirthTimeStart;
        const progress = Math.min(1.0, age / 3.0);

        // Exponential scale-in easeOut
        const scale = Math.pow(progress - 1.0, 3) + 1.0;
        group.scale.set(scale, scale, scale);

        // Flash atmosphere glow on scale transition
        const glow =
          name === "aurelia"
            ? state.cosmos.aureliaGlow
            : state.cosmos.seleneGlow;
        if (glow) {
          glow.material.opacity = (1.0 - progress) * 0.85 + 0.15;
        }

        if (progress >= 1.0) {
          group.userData.rebirthActive = false;
          group.scale.set(1.0, 1.0, 1.0);
        }
      }
    }
  });
}
