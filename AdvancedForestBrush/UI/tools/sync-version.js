const fs = require("fs");
const path = require("path");
const project = path.resolve(__dirname, "../..");
function syncVersion() {
  const props = fs.readFileSync(path.join(project, "Mod.props"), "utf8");
  const matches = [...props.matchAll(/<ModVersion>\s*([^<]+?)\s*<\/ModVersion>/g)];
  const version = matches.length === 1 ? matches[0][1].trim() : "";
  if (!/^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/.test(version) ||
      version.split(".").some(value => Number(value) > 65534)) {
    throw new Error("Mod.props: ModVersion must contain three integers from 0 to 65534, e.g. 1.0.3.");
  }
  const jsonFile = path.join(project, "UI/mod.json");
  const xmlFile = path.join(project, "Properties/PublishConfiguration.xml");
  const json = fs.readFileSync(jsonFile, "utf8");
  const xml = fs.readFileSync(xmlFile, "utf8");
  const manifest = JSON.parse(json);
  if (typeof manifest.version !== "string" ||
      (json.match(/"version"\s*:\s*"[^"]*"/g) || []).length !== 1 ||
      (xml.match(/<ModVersion\s+Value="[^"]*"\s*\/>/g) || []).length !== 1) {
    throw new Error("Cannot locate a unique version in mod.json or PublishConfiguration.xml.");
  }
  const nextJson = json.replace(/("version"\s*:\s*")[^"]*(")/, (_, before, after) => before + version + after);
  const nextXml = xml.replace(/(<ModVersion\s+Value=")[^"]*("\s*\/>)/, (_, before, after) => before + version + after);
  if (nextJson !== json) fs.writeFileSync(jsonFile, nextJson);
  if (nextXml !== xml) fs.writeFileSync(xmlFile, nextXml);
  return version;
}
module.exports = syncVersion;
if (require.main === module) console.log(`Advanced Forest Brush version: ${syncVersion()}`);
