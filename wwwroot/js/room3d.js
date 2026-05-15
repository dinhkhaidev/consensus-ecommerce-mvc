import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js";

const currencyFormatter = new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND"
});

const demoProducts = [
    {
        id: "sofa-01",
        name: "Modern Cloud Sofa",
        category: "sofa",
        price: 12900000,
        thumbnail: createThumb("sofa", "#d8b48c", "#6e4a32"),
        model3DUrl: "/models/demo-products/sofa.glb",
        scale: 1,
        offsetY: 0,
        rotationY: 0
    },
    {
        id: "table-01",
        name: "Minimal Coffee Table",
        category: "table",
        price: 3600000,
        thumbnail: createThumb("table", "#c69c6d", "#2d2219"),
        model3DUrl: "/models/demo-products/coffee-table.glb",
        scale: 1,
        offsetY: 0,
        rotationY: 0
    },
    {
        id: "chair-01",
        name: "Lounge Accent Chair",
        category: "chair",
        price: 5200000,
        thumbnail: createThumb("chair", "#bfc7b2", "#37402f"),
        model3DUrl: "/models/demo-products/lounge-chair.glb",
        scale: 1,
        offsetY: 0,
        rotationY: 0
    },
    {
        id: "lamp-01",
        name: "Warm Floor Lamp",
        category: "lamp",
        price: 1800000,
        thumbnail: createThumb("lamp", "#f2c16d", "#4d3823"),
        model3DUrl: "/models/demo-products/floor-lamp.glb",
        scale: 1,
        offsetY: 0,
        rotationY: 0
    },
    {
        id: "plant-01",
        name: "Decor Plant",
        category: "plant",
        price: 950000,
        thumbnail: createThumb("plant", "#7ca36f", "#254b2b"),
        model3DUrl: "/models/demo-products/plant.glb",
        scale: 1,
        offsetY: 0,
        rotationY: 0
    },
    {
        id: "rug-01",
        name: "Soft Area Rug",
        category: "rug",
        price: 2200000,
        thumbnail: createThumb("rug", "#d8a36f", "#9d5d30"),
        model3DUrl: null,
        scale: 1,
        offsetY: 0,
        rotationY: 0
    },
    {
        id: "cabinet-01",
        name: "Low Media Cabinet",
        category: "cabinet",
        price: 6400000,
        thumbnail: createThumb("cabinet", "#9f7b55", "#2c2118"),
        model3DUrl: "/models/demo-products/tv-stand.glb",
        scale: 1,
        offsetY: 0,
        rotationY: 0
    }
];

const state = {
    scene: null,
    camera: null,
    renderer: null,
    orbitControls: null,
    gltfLoader: null,
    raycaster: new THREE.Raycaster(),
    pointer: new THREE.Vector2(),
    dragPlane: new THREE.Plane(new THREE.Vector3(0, 1, 0), 0),
    dragPoint: new THREE.Vector3(),
    dragOffset: new THREE.Vector3(),
    roomItems: [],
    selectedObject: null,
    selectionBox: null,
    isDraggingObject: false,
    activePointerId: null,
    animationFrame: null
};

const dom = {
    canvas: document.getElementById("room3dCanvas"),
    productList: document.getElementById("productList"),
    loadingOverlay: document.getElementById("loadingOverlay"),
    selectedInfo: document.getElementById("selectedInfo"),
    totalPrice: document.getElementById("totalPrice"),
    rotateLeftBtn: document.getElementById("rotateLeftBtn"),
    rotateRightBtn: document.getElementById("rotateRightBtn"),
    deleteBtn: document.getElementById("deleteBtn"),
    snapshotBtn: document.getElementById("snapshotBtn"),
    buyRoomBtn: document.getElementById("buyRoomBtn"),
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
    renderProductList();
    bindEvents();
    updateInspector();
    updateTotal();
    handleResize();
    animate();

    setTimeout(() => dom.loadingOverlay?.classList.add("is-hidden"), 450);
}

function createScene() {
    state.scene = new THREE.Scene();
    state.scene.background = new THREE.Color(0xf4f0ea);
    state.scene.fog = new THREE.Fog(0xf4f0ea, 11, 22);
    state.gltfLoader = new GLTFLoader();
}

function createCamera() {
    state.camera = new THREE.PerspectiveCamera(45, 1, 0.1, 100);
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
    state.renderer.toneMappingExposure = 1;
    state.renderer.outputColorSpace = THREE.SRGBColorSpace;
}

function createLights() {
    const hemisphere = new THREE.HemisphereLight(0xfff5e8, 0x6c6f75, 2.2);
    state.scene.add(hemisphere);

    const keyLight = new THREE.DirectionalLight(0xfff1d2, 3.4);
    keyLight.position.set(4.8, 6.5, 4.2);
    keyLight.castShadow = true;
    keyLight.shadow.mapSize.set(2048, 2048);
    keyLight.shadow.camera.near = 0.5;
    keyLight.shadow.camera.far = 18;
    keyLight.shadow.camera.left = -7;
    keyLight.shadow.camera.right = 7;
    keyLight.shadow.camera.top = 7;
    keyLight.shadow.camera.bottom = -7;
    keyLight.shadow.bias = -0.00015;
    state.scene.add(keyLight);

    const warmFill = new THREE.PointLight(0xffbd78, 1.2, 7, 2);
    warmFill.position.set(-2.8, 2.4, 2.3);
    state.scene.add(warmFill);
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
        color: 0xc9aa84,
        roughness: 0.82,
        metalness: 0.02
    });
    const wallMat = new THREE.MeshStandardMaterial({
        color: 0xf7f0e8,
        roughness: 0.88
    });
    const trimMat = new THREE.MeshStandardMaterial({
        color: 0x6d4a32,
        roughness: 0.72
    });

    const floor = createBox(8, 0.08, 8, floorMat, [0, -0.04, 0]);
    floor.receiveShadow = true;
    floor.name = "Showroom floor";
    state.scene.add(floor);

    const backWall = createBox(8, 3.25, 0.12, wallMat, [0, 1.6, -4]);
    const leftWall = createBox(0.12, 3.25, 8, wallMat, [-4, 1.6, 0]);
    const rightNib = createBox(0.12, 3.25, 2.4, wallMat, [4, 1.6, -2.8]);
    [backWall, leftWall, rightNib].forEach(wall => {
        wall.receiveShadow = true;
        state.scene.add(wall);
    });

    createWindow(trimMat);
    createWallArt();
    createBuiltInRug();
    createFixedDecor();
}

function createWindow(trimMat) {
    const glassMat = new THREE.MeshStandardMaterial({
        color: 0xdce9ee,
        roughness: 0.2,
        metalness: 0,
        transparent: true,
        opacity: 0.64
    });

    const glass = createBox(2.25, 1.35, 0.025, glassMat, [-2.25, 1.95, -3.92]);
    const framePieces = [
        createBox(2.45, 0.07, 0.08, trimMat, [-2.25, 2.66, -3.86]),
        createBox(2.45, 0.07, 0.08, trimMat, [-2.25, 1.24, -3.86]),
        createBox(0.07, 1.45, 0.08, trimMat, [-3.48, 1.95, -3.86]),
        createBox(0.07, 1.45, 0.08, trimMat, [-1.02, 1.95, -3.86]),
        createBox(0.05, 1.35, 0.08, trimMat, [-2.25, 1.95, -3.85])
    ];
    state.scene.add(glass, ...framePieces);
}

function createWallArt() {
    const artBack = new THREE.MeshStandardMaterial({ color: 0x24201b, roughness: 0.7 });
    const artWarm = new THREE.MeshStandardMaterial({ color: 0xd47f31, roughness: 0.55 });
    const artLight = new THREE.MeshStandardMaterial({ color: 0xf1d3ad, roughness: 0.6 });
    const pieces = [
        createBox(0.92, 0.72, 0.04, artBack, [1.15, 2.08, -3.89]),
        createBox(0.62, 0.48, 0.045, artWarm, [1.12, 2.04, -3.86]),
        createBox(0.5, 0.86, 0.04, artLight, [2.05, 1.98, -3.89])
    ];
    state.scene.add(...pieces);
}

function createBuiltInRug() {
    const rugMat = new THREE.MeshStandardMaterial({
        color: 0xb97848,
        roughness: 0.94,
        metalness: 0.01
    });
    const rug = createBox(3.25, 0.035, 2.25, rugMat, [0.8, 0.02, 0.82]);
    rug.receiveShadow = true;
    rug.name = "Showroom fixed rug";
    state.scene.add(rug);
}

function createFixedDecor() {
    const lampProduct = {
        id: "fixed-lamp",
        name: "Ambient showroom lamp",
        category: "lamp",
        price: 0
    };
    const plantProduct = {
        id: "fixed-plant",
        name: "Showroom plant",
        category: "plant",
        price: 0
    };
    const lamp = createLampPlaceholder(lampProduct, true);
    lamp.position.set(-3.25, 0, -2.6);
    lamp.rotation.y = THREE.MathUtils.degToRad(-12);
    const plant = createPlantPlaceholder(plantProduct, true);
    plant.position.set(3.2, 0, -3.1);
    plant.scale.setScalar(0.9);
    state.scene.add(lamp, plant);
}

function renderProductList() {
    dom.productList.innerHTML = "";

    demoProducts.forEach(product => {
        const card = document.createElement("article");
        card.className = "room-product-card";
        card.innerHTML = `
            <div class="product-thumb">
                ${product.thumbnail ? `<img src="${product.thumbnail}" alt="${escapeHtml(product.name)}">` : categoryIcon(product.category)}
            </div>
            <div class="product-meta">
                <small>${escapeHtml(product.category)}</small>
                <h3>${escapeHtml(product.name)}</h3>
                <div class="product-card-footer">
                    <span class="product-price">${currencyFormatter.format(product.price || 0)}</span>
                    <button type="button" class="room3d-btn room3d-btn-primary add-product-btn">Add</button>
                </div>
            </div>
        `;

        const image = card.querySelector("img");
        image?.addEventListener("error", () => {
            image.replaceWith(iconElement(product.category));
        });

        card.querySelector("button")?.addEventListener("click", () => addProductToRoom(product, card));
        dom.productList.appendChild(card);
    });
}

async function addProductToRoom(product, card) {
    const button = card?.querySelector("button");
    button?.setAttribute("disabled", "disabled");
    if (button) button.textContent = "Loading";

    try {
        let object = null;
        const canLoadModel = await shouldLoadModel(product.model3DUrl);

        if (canLoadModel) {
            try {
                object = await loadModel(product);
            } catch (error) {
                console.warn(`Room3D model fallback for ${product.id}:`, error);
            }
        }

        if (!object) {
            object = createPlaceholderProduct(product);
        }

        placeNewObject(object, product);
        showToast(`${product.name} đã được đặt vào phòng.`);
    } finally {
        button?.removeAttribute("disabled");
        if (button) button.textContent = "Add";
    }
}

async function shouldLoadModel(model3DUrl) {
    if (!model3DUrl) return false;

    try {
        const response = await fetch(model3DUrl, { method: "HEAD" });
        return response.ok;
    } catch {
        return false;
    }
}

function loadModel(product) {
    return new Promise((resolve, reject) => {
        state.gltfLoader.load(
            product.model3DUrl,
            gltf => {
                const group = new THREE.Group();
                group.add(gltf.scene);
                prepareObjectMeshes(group);
                normalizeModel(group, product);
                applyRoomItemMeta(group, product, false);
                resolve(group);
            },
            undefined,
            reject
        );
    });
}

function normalizeModel(group, product) {
    group.updateMatrixWorld(true);
    const box = new THREE.Box3().setFromObject(group);
    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());
    const maxSize = Math.max(size.x, size.y, size.z) || 1;
    const fitScale = maxSize > 2.4 ? 2.4 / maxSize : 1;

    group.children[0].position.x -= center.x;
    group.children[0].position.y -= box.min.y;
    group.children[0].position.z -= center.z;
    group.rotation.y = product.rotationY || 0;
    group.scale.setScalar((product.scale || 1) * fitScale);
}

function placeNewObject(object, product) {
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
    object.position.set(spawn[0], product.offsetY || spawn[1], spawn[2]);
    object.userData.instanceId = `${product.id}-${Date.now()}-${Math.round(Math.random() * 1000)}`;
    object.userData.baseScale = object.scale.x || 1;
    object.userData.spawnStart = performance.now();
    object.scale.multiplyScalar(0.82);

    state.scene.add(object);
    state.roomItems.push({
        instanceId: object.userData.instanceId,
        productId: product.id,
        name: product.name,
        category: product.category,
        price: product.price || 0,
        object3D: object,
        quantity: 1
    });

    selectObject(object);
    updateTotal();
}

function createPlaceholderProduct(product) {
    let group;
    switch ((product.category || "").toLowerCase()) {
        case "sofa":
            group = createSofaPlaceholder(product);
            break;
        case "table":
            group = createTablePlaceholder(product);
            break;
        case "chair":
            group = createChairPlaceholder(product);
            break;
        case "lamp":
            group = createLampPlaceholder(product);
            break;
        case "plant":
            group = createPlantPlaceholder(product);
            break;
        case "rug":
            group = createRugPlaceholder(product);
            break;
        case "cabinet":
            group = createCabinetPlaceholder(product);
            break;
        default:
            group = createDefaultPlaceholder(product);
            break;
    }

    applyRoomItemMeta(group, product, true);
    prepareObjectMeshes(group);
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

function createLampPlaceholder(product, fixed = false) {
    const group = new THREE.Group();
    const metal = new THREE.MeshStandardMaterial({ color: 0x3a3027, roughness: 0.48, metalness: 0.28 });
    const shade = new THREE.MeshStandardMaterial({ color: 0xf2c98b, roughness: 0.7, emissive: 0x503318, emissiveIntensity: 0.18 });
    group.add(createCylinder(0.28, 0.28, 0.06, metal, [0, 0.03, 0]));
    group.add(createCylinder(0.035, 0.035, 1.55, metal, [0, 0.82, 0]));
    group.add(createCylinder(0.28, 0.42, 0.48, shade, [0, 1.72, 0]));
    const glow = new THREE.PointLight(0xffbb76, fixed ? 1.1 : 0.75, 3.2, 2);
    glow.position.set(0, 1.55, 0);
    group.add(glow);
    if (product) applyRoomItemMeta(group, product, !fixed);
    return group;
}

function createPlantPlaceholder(product, fixed = false) {
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
    if (product) applyRoomItemMeta(group, product, !fixed);
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

function createDefaultPlaceholder() {
    const group = new THREE.Group();
    const mat = new THREE.MeshStandardMaterial({ color: 0xb9a48d, roughness: 0.86 });
    group.add(createBox(1, 0.8, 1, mat, [0, 0.4, 0]));
    return group;
}

function applyRoomItemMeta(group, product, isPlaceholder) {
    group.userData = {
        ...group.userData,
        type: "room-item",
        productId: product.id,
        name: product.name,
        category: product.category,
        price: product.price || 0,
        isPlaceholder,
        offsetY: product.offsetY || 0
    };
}

function bindEvents() {
    dom.canvas.addEventListener("pointerdown", handlePointerDown);
    dom.canvas.addEventListener("pointermove", handlePointerMove);
    window.addEventListener("pointerup", handlePointerUp);
    window.addEventListener("resize", handleResize);

    dom.rotateLeftBtn?.addEventListener("click", () => rotateSelected(-15));
    dom.rotateRightBtn?.addEventListener("click", () => rotateSelected(15));
    dom.deleteBtn?.addEventListener("click", deleteSelected);
    dom.snapshotBtn?.addEventListener("click", takeSnapshot);
    dom.buyRoomBtn?.addEventListener("click", buyRoom);
}

function handlePointerDown(event) {
    setPointerFromEvent(event);
    const hitObject = getRoomItemFromPointer();

    if (!hitObject) {
        selectObject(null);
        return;
    }

    selectObject(hitObject);
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

function handlePointerUp(event) {
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

    if (object) {
        state.selectionBox = new THREE.BoxHelper(object, 0xd47f31);
        state.selectionBox.material.depthTest = false;
        state.selectionBox.renderOrder = 999;
        state.scene.add(state.selectionBox);
    }

    updateInspector();
}

function rotateSelected(degrees) {
    if (!state.selectedObject) {
        showToast("Hãy chọn một sản phẩm trong phòng trước.");
        return;
    }

    state.selectedObject.rotation.y += THREE.MathUtils.degToRad(degrees);
    updateSelectionBox();
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
    [dom.rotateLeftBtn, dom.rotateRightBtn, dom.deleteBtn].forEach(btn => {
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
        <span>${escapeHtml(object.userData.category || "furniture")}${object.userData.isPlaceholder ? " · 3D placeholder" : " · GLB model"}</span>
        <span class="selected-price">${currencyFormatter.format(object.userData.price || 0)}</span>
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
    const summary = `Đã chọn ${state.roomItems.length} sản phẩm, tổng ${currencyFormatter.format(total)}. Tính năng checkout bộ phòng sẽ được kết nối sau.`;
    showToast(summary);
    // TODO: Later POST roomItems to /Cart/AddRoomItems when the backend endpoint exists.
}

function animate() {
    state.animationFrame = requestAnimationFrame(animate);
    state.orbitControls?.update();
    animateSpawnedItems();
    updateSelectionBox();
    state.renderer.render(state.scene, state.camera);
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

function updateSelectionBox() {
    state.selectionBox?.update();
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

function categoryIcon(category) {
    const icon = {
        sofa: "bi-lamp",
        table: "bi-grid-3x3-gap",
        chair: "bi-easel",
        lamp: "bi-lightbulb",
        plant: "bi-flower1",
        rug: "bi-border-style",
        cabinet: "bi-collection"
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
            <text x="80" y="88" text-anchor="middle" font-family="Arial, sans-serif" font-size="22" font-weight="700" fill="${fg}">${label}</text>
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
