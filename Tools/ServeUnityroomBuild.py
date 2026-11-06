"""Preview the actual Gzip release on localhost with the correct HTTP headers."""
import argparse
import functools
import mimetypes
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path


class UnityHandler(SimpleHTTPRequestHandler):
    def guess_type(self, path):
        uncompressed = path[:-3] if path.endswith(".gz") else path
        if uncompressed.endswith(".wasm"):
            return "application/wasm"
        if uncompressed.endswith(".data"):
            return "application/octet-stream"
        return mimetypes.guess_type(uncompressed)[0] or "application/octet-stream"

    def send_head(self):
        path = Path(self.translate_path(self.path))
        self.gzip_file = path.is_file() and path.suffix == ".gz"
        return super().send_head()

    def end_headers(self):
        if getattr(self, "gzip_file", False):
            self.send_header("Content-Encoding", "gzip")
        self.send_header("Cache-Control", "no-cache")
        super().end_headers()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--directory", default="Builds/Unityroom/ShelfCare")
    parser.add_argument("--port", type=int, default=8765)
    args = parser.parse_args()
    directory = Path(args.directory).resolve()
    if not (directory / "index.html").is_file():
        parser.error(f"Build index.html not found: {directory}")
    handler = functools.partial(UnityHandler, directory=str(directory))
    server = ThreadingHTTPServer(("127.0.0.1", args.port), handler)
    print(f"Shelf Care preview: http://127.0.0.1:{args.port}/", flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
