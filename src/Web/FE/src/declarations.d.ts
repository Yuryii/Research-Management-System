declare module '*.json' {
  const value: any;
  export default value;
}
declare interface FileParameter {
  data: Blob;
  fileName: string;
}
