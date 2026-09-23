import { describe, expect, it } from "vitest";
import { fieldsFor, numberScopesFor } from "./numberQuery";

describe("fieldsFor", () => {
  it("offers what a unit can measure", () => {
    expect(fieldsFor("words", "single")).toEqual(["number", "letters", "uniqueLetters", "value", "frequency", "occurrence"]);
    expect(fieldsFor("verses", "single")).toEqual(["number", "words", "letters", "uniqueLetters", "value", "frequency", "occurrence"]);
    expect(fieldsFor("verses", "range")).toEqual(["number", "verses", "words", "letters", "uniqueLetters", "value"]);
    expect(fieldsFor("sentences", "single")).toEqual(["words", "letters", "uniqueLetters", "value"]);
    expect(fieldsFor("chapters", "set")).toEqual(["number", "verses", "words", "letters", "uniqueLetters", "value"]);
  });
});

describe("numberScopesFor", () => {
  it("reads a word's number in its verse, chapter or book", () => {
    expect(numberScopesFor("words").map((s) => s.value)).toEqual(["verse", "chapter", "book"]);
    expect(numberScopesFor("verses").map((s) => s.value)).toEqual(["chapter", "book"]);
    expect(numberScopesFor("pages")).toEqual([]);
  });
});
