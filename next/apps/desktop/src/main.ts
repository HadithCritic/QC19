// One Arabic font throughout, for the Quran text and every other Arabic word
// alike (ADR 0004).
import "@fontsource/amiri-quran/400.css";
import "@fontsource-variable/instrument-sans";
import "@fontsource/ibm-plex-mono/400.css";
import "@fontsource/ibm-plex-mono/500.css";
import "./styles/tokens.css";
import "./styles/base.css";
import { mount } from "svelte";
import App from "./App.svelte";

const target = document.getElementById("app");
if (!target) throw new Error("index.html has no #app element");

export default mount(App, { target });
