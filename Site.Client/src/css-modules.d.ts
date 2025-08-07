// This file tells TypeScript how to handle CSS module imports.
declare module '*.module.css' {
    const classes: { readonly [key: string]: string };
    export default classes;
}