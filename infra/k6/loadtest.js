import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    users_1000: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "2m", target: 200 },
        { duration: "2m", target: 500 },
        { duration: "2m", target: 1000 },
        { duration: "5m", target: 1000 },
        { duration: "1m", target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<1000"],
  },
};

const BASE_URL = __ENV.BASE_URL || "https://host.docker.internal:7098";
const API = "/api";
const EMAIL = __ENV.EMAIL || "vlad@gmail.com";        // NICHT hardcoden
const PASSWORD = __ENV.PASSWORD || "vvvHitmenPRO321";  // NICHT hardcoden

const ACCESS_COOKIE_NAME = "accessToken";

export function setup() {
  const jar = http.cookieJar();

  const loginRes = http.post(
    `${BASE_URL}${API}/auth/login`,
    JSON.stringify({ email: EMAIL, password: PASSWORD }),
    { headers: { "Content-Type": "application/json" }, jar }
  );

  check(loginRes, { "login ok": (r) => r.status === 200 });

  // Cookie aus Jar lesen (Origin ohne /api)
  const json = loginRes.json();
  const token = json?.value?.token;

  if (!token) {
    throw new Error(`Kein JWT im Login-JSON gefunden. Body: ${loginRes.body}`);
  }

  

  return { token };
}

export default function (data) {
  const headers = { Authorization: `Bearer ${data.token}` };

  const todosRes = http.get(`${BASE_URL}${API}/todos`, { headers });

  const ok = check(todosRes, { "todos ok": (r) => r.status === 200 });

  if (!ok) {
    // nur manchmal loggen, sonst zu viel
    if (Math.random() < 0.02) {
      console.log(`TODOS FAIL status=${todosRes.status} body=${todosRes.body}`);
    }
  }
  sleep(2 + Math.random() * 6);
}
