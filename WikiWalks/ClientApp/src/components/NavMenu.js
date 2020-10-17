import React from "react";
import { Link } from "react-router-dom";
import { Button, Container, Navbar, NavbarBrand } from "reactstrap";
import "./NavMenu.css";

export default function NavMenu() {
    return (
        <header>
            <Navbar
                className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3"
                light
            >
                <Container>
                    <NavbarBrand
                        tag={Link}
                        to="/"
                        style={{ fontWeight: "bold" }}
                    >
                        うぃき忍者
                    </NavbarBrand>
                    <Button
                        color="primary"
                        style={{ fontWeight: "bold" }}
                        href="https://wiki.lingual-ninja.com"
                        target="_blank"
                        rel="noopener noreferrer"
                    >
                        English
                    </Button>
                </Container>
            </Navbar>
        </header>
    );
}
