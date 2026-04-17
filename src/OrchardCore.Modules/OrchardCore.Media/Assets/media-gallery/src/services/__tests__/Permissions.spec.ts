import { describe, expect, it } from "vitest";
import { usePermissions, supportsUpload, supportsDelete } from "../Permissions";

const { canManage, canManageFolder } = usePermissions();

describe("Permissions", () => {
  it("canManage always returns true", () => {
    expect(canManage.value).toBe(true);
  });

  it("canManageFolder always returns true for any directory", () => {
    expect(canManageFolder("/")).toBe(true);
    expect(canManageFolder("/Images")).toBe(true);
    expect(canManageFolder("/some/deep/path")).toBe(true);
  });

  it("supportsUpload always returns true", () => {
    expect(supportsUpload()).toBe(true);
  });

  it("supportsDelete always returns true", () => {
    expect(supportsDelete()).toBe(true);
  });
});
