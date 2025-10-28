import fs from "fs";
import https from "https";
import next from "next";

const dev = process.env.NODE_ENV !== "production";
const app = next({ dev });
const handle = app.getRequestHandler();


const httpsOptions = {
  key: fs.readFileSync("../certs/localhost-key.pem"),
  cert: fs.readFileSync("../certs/localhost.pem"), // "mkcert localhost" in certs folder
};

app.prepare().then(() => {
  https.createServer(httpsOptions, (req, res) => {
    handle(req, res);
  }).listen(3000, (err) => {
    if (err) throw err;
    console.log("🚀 HTTPS Dev Server running on https://localhost:3000");
  });
});